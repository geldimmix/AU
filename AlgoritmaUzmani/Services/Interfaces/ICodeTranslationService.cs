namespace AlgoritmaUzmani.Services.Interfaces;

public interface ICodeTranslationService
{
    Task<Dictionary<string, string>> TranslateCodeToMultipleLanguagesAsync(string sourceCode, string sourceLanguage, List<string> targetLanguages);
    Task<string> TranslateCodeAsync(string sourceCode, string sourceLanguage, string targetLanguage);
}

