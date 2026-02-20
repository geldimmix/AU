using AlgoritmaUzmani.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AlgoritmaUzmani.Services;

public class TranslationService : ITranslationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TranslationService> _logger;

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

    /// <summary>
    /// Translates a SINGLE piece of text. Caller must ensure it fits within token limits.
    /// For large content, caller should split into chunks before calling this.
    /// </summary>
    public async Task<string> TranslateToEnglishAsync(string turkishText)
    {
        if (string.IsNullOrWhiteSpace(turkishText))
            return string.Empty;

        var apiKey = _configuration["DeepInfra:ApiKey"];
        var baseUrl = _configuration["DeepInfra:BaseUrl"];
        var model = _configuration["DeepInfra:Model"];

        _logger.LogInformation("TranslateToEnglishAsync: {Len} chars gönderiliyor", turkishText.Length);

        try
        {
            var request = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = SystemPrompt },
                    new { role = "user", content = turkishText }
                },
                temperature = 0.3
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _httpClient.PostAsync(baseUrl, content);
            var responseJson = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("API Response: {Status}", response.StatusCode);

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
            _logger.LogError(ex, "Translation FAILED for {Len} chars", turkishText.Length);
            throw;
        }
    }

    public async Task<(string title, string content, string? summary, string? metaDescription)> TranslateGuideAsync(
        string titleTr, string contentTr, string? summaryTr, string? metaDescriptionTr)
    {
        // This is only used by CreateGuide - NOT for large content translation
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

            var cleanContent = System.Text.RegularExpressions.Regex.Replace(content ?? "", "<[^>]*>", " ");
            cleanContent = System.Text.RegularExpressions.Regex.Replace(cleanContent, @"\s+", " ").Trim();

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

            var metaDescription = string.Empty;
            var keywords = new List<string>();

            var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.StartsWith("META_DESCRIPTION:", StringComparison.OrdinalIgnoreCase))
                {
                    metaDescription = line.Substring("META_DESCRIPTION:".Length).Trim().Trim('[', ']');
                }
                else if (line.StartsWith("KEYWORDS:", StringComparison.OrdinalIgnoreCase))
                {
                    var keywordsStr = line.Substring("KEYWORDS:".Length).Trim().Trim('[', ']');
                    keywords = keywordsStr.Split(',')
                        .Select(k => k.Trim())
                        .Where(k => !string.IsNullOrEmpty(k))
                        .Take(5)
                        .ToList();
                }
            }

            if (!string.IsNullOrEmpty(metaDescription) && metaDescription.Length > 160)
                metaDescription = metaDescription.Substring(0, 157).TrimEnd() + "...";

            return (metaDescription, keywords);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SEO suggestion generation failed for title: {Title}", title);
            throw;
        }
    }
}
