namespace AlgoritmaUzmani.Services.Interfaces;

public interface ITranslationService
{
    Task<string> TranslateToEnglishAsync(string turkishText);
    Task<(string title, string content, string? summary, string? metaDescription)> TranslateGuideAsync(
        string titleTr, string contentTr, string? summaryTr, string? metaDescriptionTr);
    Task<(string metaDescription, List<string> keywords)> GenerateSeoSuggestionsAsync(string title, string content);
}

