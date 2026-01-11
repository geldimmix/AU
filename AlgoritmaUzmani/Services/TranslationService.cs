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

        try
        {
            var apiKey = _configuration["DeepInfra:ApiKey"];
            var baseUrl = _configuration["DeepInfra:BaseUrl"];
            var model = _configuration["DeepInfra:Model"];

            var request = new
            {
                model = model,
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = @"You are a professional translator specializing in technical content. Your task is to translate Turkish text to English.

CRITICAL RULES:
1. PRESERVE ALL HTML TAGS EXACTLY as they are (<h2>, <h3>, <p>, <strong>, <em>, <ul>, <li>, <pre>, <code>, etc.)
2. Only translate the TEXT CONTENT between HTML tags
3. Keep all HTML attributes unchanged (class, style, id, etc.)
4. Maintain the exact same document structure
5. Do not add or remove any HTML tags
6. Output ONLY the translated content, no explanations

Example:
Input: <h2>Veri Yapıları Nedir?</h2><p>Veri yapıları önemlidir.</p>
Output: <h2>What are Data Structures?</h2><p>Data structures are important.</p>"
                    },
                    new
                    {
                        role = "user",
                        content = turkishText
                    }
                },
                temperature = 0.3
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _httpClient.PostAsync(baseUrl, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);
            
            var translatedText = document.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return translatedText ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Translation failed for text: {Text}", turkishText.Substring(0, Math.Min(50, turkishText.Length)));
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

