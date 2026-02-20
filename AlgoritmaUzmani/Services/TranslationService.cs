using AlgoritmaUzmani.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AlgoritmaUzmani.Services;

public class TranslationService : ITranslationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TranslationService> _logger;
    
    // Conservative limit: Turkish+HTML ≈ 3-4 tokens per char, so 4000 chars ≈ 16000 tokens max
    private const int MaxChunkChars = 4000;

    private static readonly string SystemPrompt = @"You are a professional Turkish to English translator for technical documentation.

ABSOLUTE RULES - NEVER BREAK THESE:
1. Return ONLY the translated HTML - no explanations, no markdown, no code blocks
2. PRESERVE ALL HTML TAGS EXACTLY: <h1>, <h2>, <h3>, <h4>, <p>, <ul>, <li>, <ol>, <strong>, <em>, <code>, <pre>, <blockquote>, <a>, <img>, <table>, <tr>, <td>, <th>, <div>, <span>, etc.
3. HTML tags must remain as literal HTML, never escaped or encoded
4. Only translate the text BETWEEN tags
5. Keep ALL attributes unchanged (class, id, href, src, style, etc.)
6. Keep code inside <code> and <pre> tags UNTRANSLATED
7. Keep URLs, email addresses, and technical terms as-is
8. Maintain exact whitespace and line breaks

EXAMPLES:
Input: <h2>1. Giriş</h2><p>Bu bir <strong>örnek</strong> metindir.</p>
Output: <h2>1. Introduction</h2><p>This is an <strong>example</strong> text.</p>

Input: <h3>Veri Yapıları</h3><ul><li>Diziler</li><li>Bağlı Listeler</li></ul>
Output: <h3>Data Structures</h3><ul><li>Arrays</li><li>Linked Lists</li></ul>

Input: <pre><code>def hello(): print('Merhaba')</code></pre>
Output: <pre><code>def hello(): print('Merhaba')</code></pre>

RETURN ONLY THE TRANSLATED HTML, NOTHING ELSE.";

    public TranslationService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<TranslationService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> TranslateToEnglishAsync(string turkishText)
    {
        if (string.IsNullOrWhiteSpace(turkishText))
            return string.Empty;

        _logger.LogInformation("=== TRANSLATION START === Input length: {Len} chars", turkishText.Length);

        // If content is short enough, translate directly
        if (turkishText.Length <= MaxChunkChars)
        {
            return await TranslateChunkAsync(turkishText);
        }

        // Split into chunks and translate each
        var chunks = SplitHtmlIntoChunks(turkishText);
        _logger.LogInformation("Content too large, split into {Count} chunks", chunks.Count);

        var translatedChunks = new List<string>();
        for (int i = 0; i < chunks.Count; i++)
        {
            _logger.LogInformation("Translating chunk {Current}/{Total} ({Len} chars)...", 
                i + 1, chunks.Count, chunks[i].Length);
            
            var translated = await TranslateChunkAsync(chunks[i]);
            translatedChunks.Add(translated);
            
            _logger.LogInformation("Chunk {Current}/{Total} translated successfully", i + 1, chunks.Count);
            
            // Small delay between chunks to avoid rate limiting
            if (i < chunks.Count - 1)
                await Task.Delay(500);
        }

        var result = string.Join("\n", translatedChunks);
        _logger.LogInformation("=== ALL CHUNKS TRANSLATED === Total result: {Len} chars", result.Length);
        return result;
    }

    /// <summary>
    /// Splits HTML content into chunks at block-level element boundaries
    /// </summary>
    private List<string> SplitHtmlIntoChunks(string html)
    {
        _logger.LogInformation("SplitHtmlIntoChunks called, input length: {Len}", html.Length);
        
        // Split at block-level HTML element boundaries
        var blockPattern = @"(?=<(?:h[1-6]|p|div|ul|ol|table|pre|blockquote|section|article|hr|figure|li|tr)[\s>/])";
        var segments = Regex.Split(html, blockPattern, RegexOptions.IgnoreCase)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();

        _logger.LogInformation("Split into {Count} HTML segments", segments.Length);

        // If regex didn't split anything useful, force-split by closing tags or newlines
        if (segments.Length <= 1)
        {
            _logger.LogWarning("HTML split produced only {Count} segment(s), trying alternative split", segments.Length);
            // Try splitting by </p>, </div>, </li>, etc.
            var altPattern = @"(</(?:p|div|h[1-6]|li|tr|ul|ol|table|pre|blockquote)>)";
            var altParts = Regex.Split(html, altPattern, RegexOptions.IgnoreCase)
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();
            
            if (altParts.Length > 1)
            {
                // Rejoin closing tags with their preceding content
                var rejoined = new List<string>();
                var temp = new StringBuilder();
                foreach (var part in altParts)
                {
                    temp.Append(part);
                    if (Regex.IsMatch(part, @"^</(?:p|div|h[1-6]|li|tr|ul|ol|table|pre|blockquote)>$", RegexOptions.IgnoreCase))
                    {
                        rejoined.Add(temp.ToString());
                        temp.Clear();
                    }
                }
                if (temp.Length > 0)
                    rejoined.Add(temp.ToString());
                
                segments = rejoined.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                _logger.LogInformation("Alternative split produced {Count} segments", segments.Length);
            }
        }

        // ULTIMATE FALLBACK: if still no segments, hard-split by character count
        if (segments.Length <= 1)
        {
            _logger.LogWarning("All HTML splits failed, doing hard character split");
            return HardSplitByChars(html);
        }

        var chunks = new List<string>();
        var currentChunk = new StringBuilder();

        foreach (var segment in segments)
        {
            if (currentChunk.Length > 0 && currentChunk.Length + segment.Length > MaxChunkChars)
            {
                chunks.Add(currentChunk.ToString());
                currentChunk.Clear();
            }

            // If a single segment is still too large, hard-split it
            if (currentChunk.Length == 0 && segment.Length > MaxChunkChars)
            {
                chunks.AddRange(HardSplitByChars(segment));
                continue;
            }

            currentChunk.Append(segment);
        }

        if (currentChunk.Length > 0)
            chunks.Add(currentChunk.ToString());

        _logger.LogInformation("Final chunk count: {Count}, sizes: {Sizes}", 
            chunks.Count, string.Join(", ", chunks.Select(c => c.Length)));

        return chunks;
    }

    /// <summary>
    /// Hard-splits text by character limit as last resort
    /// </summary>
    private List<string> HardSplitByChars(string text)
    {
        var chunks = new List<string>();
        for (int i = 0; i < text.Length; i += MaxChunkChars)
        {
            var end = Math.Min(i + MaxChunkChars, text.Length);
            
            // Try to break at a tag boundary to avoid splitting HTML tags
            if (end < text.Length)
            {
                var searchStart = Math.Max(i + (MaxChunkChars / 2), i); // don't go below half
                var lastTagClose = text.LastIndexOf('>', end);
                if (lastTagClose > searchStart)
                    end = lastTagClose + 1;
            }
            
            chunks.Add(text.Substring(i, end - i));
            i = end - MaxChunkChars; // adjust for next iteration since end may have moved
        }
        
        // Simpler fallback if above logic fails
        if (chunks.Count == 0 || chunks.Any(c => c.Length > MaxChunkChars * 2))
        {
            chunks.Clear();
            for (int i = 0; i < text.Length; i += MaxChunkChars)
            {
                var len = Math.Min(MaxChunkChars, text.Length - i);
                chunks.Add(text.Substring(i, len));
            }
        }
        
        return chunks;
    }

    /// <summary>
    /// Translates a single chunk (must be under token limit)
    /// </summary>
    private async Task<string> TranslateChunkAsync(string text)
    {
        var apiKey = _configuration["DeepInfra:ApiKey"];
        var baseUrl = _configuration["DeepInfra:BaseUrl"];
        var model = _configuration["DeepInfra:Model"];

        try
        {
            var request = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = SystemPrompt },
                    new { role = "user", content = text }
                },
                temperature = 0.3
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _httpClient.PostAsync(baseUrl, content);
            var responseJson = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation("Chunk response status: {Status}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("API Error! Status: {Status}, Body: {Body}", response.StatusCode, responseJson);
                throw new Exception($"DeepInfra API error: {response.StatusCode} - {responseJson}");
            }

            using var document = JsonDocument.Parse(responseJson);
            
            var translatedText = document.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;

            // Post-process: Fix any accidentally escaped HTML tags
            translatedText = translatedText
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&quot;", "\"")
                .Replace("&amp;", "&")
                .Replace("&#39;", "'");
            
            // Remove markdown code block wrappers if AI added them
            if (translatedText.StartsWith("```html"))
            {
                translatedText = translatedText.Substring(7);
                if (translatedText.EndsWith("```"))
                    translatedText = translatedText.Substring(0, translatedText.Length - 3);
            }
            else if (translatedText.StartsWith("```"))
            {
                var firstNewLine = translatedText.IndexOf('\n');
                if (firstNewLine > 0)
                    translatedText = translatedText.Substring(firstNewLine + 1);
                if (translatedText.EndsWith("```"))
                    translatedText = translatedText.Substring(0, translatedText.Length - 3);
            }

            return translatedText.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "=== CHUNK TRANSLATION FAILED === Input: {Text}", text.Substring(0, Math.Min(100, text.Length)));
            throw;
        }
    }

    public async Task<(string title, string content, string? summary, string? metaDescription)> TranslateGuideAsync(
        string titleTr, string contentTr, string? summaryTr, string? metaDescriptionTr)
    {
        var titleTask = TranslateToEnglishAsync(titleTr);
        var contentTask = TranslateToEnglishAsync(contentTr);
        var summaryTask = !string.IsNullOrEmpty(summaryTr) 
            ? TranslateToEnglishAsync(summaryTr) 
            : Task.FromResult<string>(null!);
        var metaTask = !string.IsNullOrEmpty(metaDescriptionTr) 
            ? TranslateToEnglishAsync(metaDescriptionTr) 
            : Task.FromResult<string>(null!);

        await Task.WhenAll(titleTask, contentTask, summaryTask, metaTask);

        return (
            await titleTask,
            await contentTask,
            await summaryTask,
            await metaTask
        );
    }

    public async Task<(string metaDescription, List<string> keywords)> GenerateSeoSuggestionsAsync(string title, string content)
    {
        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(content))
            return (string.Empty, new List<string>());

        try
        {
            var apiKey = _configuration["DeepInfra:ApiKey"];
            var baseUrl = _configuration["DeepInfra:BaseUrl"];
            var model = _configuration["DeepInfra:Model"];

            // Strip HTML tags for cleaner analysis
            var cleanContent = System.Text.RegularExpressions.Regex.Replace(content ?? "", "<[^>]*>", " ");
            cleanContent = System.Text.RegularExpressions.Regex.Replace(cleanContent, @"\s+", " ").Trim();
            
            // Limit content length
            if (cleanContent.Length > 2000)
                cleanContent = cleanContent.Substring(0, 2000);

            var prompt = $@"Aşağıdaki içerik için SEO önerileri oluştur:

Başlık: {title}

İçerik: {cleanContent}

Lütfen şu formatta yanıt ver (sadece bu formatı kullan, başka açıklama ekleme):
META_DESCRIPTION: [Tam olarak 150-160 karakter arasında, Türkçe, içeriği özetleyen ve tıklamaya teşvik eden bir meta açıklama yaz]
KEYWORDS: [Virgülle ayrılmış 5 adet Türkçe anahtar kelime]";

            var request = new
            {
                model = model,
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = "Sen bir SEO uzmanısın. İçerikleri analiz edip meta description ve anahtar kelime önerileri sunuyorsun. Sadece istenen formatta yanıt ver."
                    },
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(request);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _httpClient.PostAsync(baseUrl, httpContent);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);
            
            var result = document.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;

            // Parse the response
            var metaDescription = string.Empty;
            var keywords = new List<string>();

            var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.StartsWith("META_DESCRIPTION:", StringComparison.OrdinalIgnoreCase))
                {
                    metaDescription = line.Substring("META_DESCRIPTION:".Length).Trim();
                    // Remove brackets if present
                    metaDescription = metaDescription.Trim('[', ']');
                }
                else if (line.StartsWith("KEYWORDS:", StringComparison.OrdinalIgnoreCase))
                {
                    var keywordsStr = line.Substring("KEYWORDS:".Length).Trim();
                    keywordsStr = keywordsStr.Trim('[', ']');
                    keywords = keywordsStr.Split(',')
                        .Select(k => k.Trim())
                        .Where(k => !string.IsNullOrEmpty(k))
                        .Take(5)
                        .ToList();
                }
            }

            // Ensure meta description is within limits (strict 160 char limit)
            if (!string.IsNullOrEmpty(metaDescription))
            {
                // Remove any extra whitespace
                metaDescription = metaDescription.Trim();
                
                if (metaDescription.Length > 160)
                {
                    // Cut at 157 and add "..."
                    metaDescription = metaDescription.Substring(0, 157).TrimEnd() + "...";
                }
            }

            return (metaDescription, keywords);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SEO suggestion generation failed for title: {Title}", title);
            throw;
        }
    }
}

