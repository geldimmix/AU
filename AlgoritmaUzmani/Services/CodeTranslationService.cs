using AlgoritmaUzmani.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AlgoritmaUzmani.Services;

public class CodeTranslationService : ICodeTranslationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CodeTranslationService> _logger;

    public CodeTranslationService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<CodeTranslationService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Dictionary<string, string>> TranslateCodeToMultipleLanguagesAsync(
        string sourceCode, 
        string sourceLanguage, 
        List<string> targetLanguages)
    {
        var result = new Dictionary<string, string>();
        
        // Source code'u da ekle
        result[sourceLanguage] = sourceCode;

        // Her dil için çeviri yap
        foreach (var targetLang in targetLanguages)
        {
            if (targetLang.ToLower() == sourceLanguage.ToLower())
                continue;

            try
            {
                var translated = await TranslateCodeAsync(sourceCode, sourceLanguage, targetLang);
                result[targetLang] = translated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error translating code to {targetLang}");
                result[targetLang] = $"// Translation error: {ex.Message}\n{sourceCode}";
            }
        }

        return result;
    }

    public async Task<string> TranslateCodeAsync(string sourceCode, string sourceLanguage, string targetLanguage)
    {
        if (string.IsNullOrWhiteSpace(sourceCode))
            return string.Empty;

        try
        {
            var apiKey = _configuration["DeepInfra:ApiKey"];
            var baseUrl = _configuration["DeepInfra:BaseUrl"];
            var model = _configuration["DeepInfra:Model"];

            var systemPrompt = $@"You are an expert programmer skilled in translating code between programming languages.

TASK: Translate the provided {sourceLanguage} code to {targetLanguage}.

CRITICAL RULES:
1. Preserve the EXACT SAME LOGIC and functionality
2. Use idiomatic {targetLanguage} syntax and conventions
3. Keep variable/function names meaningful (translate comments but keep code names in English)
4. Add brief comments where the translation might not be obvious
5. Output ONLY the translated code, no explanations or markdown formatting
6. Do NOT wrap the code in markdown code blocks
7. Preserve code structure and indentation
8. If certain concepts don't exist in target language, use the closest equivalent and add a comment

Example - Python to JavaScript:
Input (Python):
def calculate_sum(numbers):
    total = 0
    for num in numbers:
        total += num
    return total

Output (JavaScript):
function calculateSum(numbers) {{
    let total = 0;
    for (const num of numbers) {{
        total += num;
    }}
    return total;
}}";

            var request = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = $"Translate this {sourceLanguage} code to {targetLanguage}:\n\n{sourceCode}" }
                },
                temperature = 0.3,
                max_tokens = 4000
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _httpClient.PostAsync(baseUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"DeepInfra API Error: {response.StatusCode} - {responseBody}");
                throw new Exception($"API Error: {response.StatusCode}");
            }

            var jsonResponse = JsonDocument.Parse(responseBody);
            var translatedCode = jsonResponse.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return translatedCode?.Trim() ?? sourceCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Code translation error");
            throw;
        }
    }
}

