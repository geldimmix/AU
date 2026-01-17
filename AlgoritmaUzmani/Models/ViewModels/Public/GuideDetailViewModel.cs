using AlgoritmaUzmani.Models.Entities;

namespace AlgoritmaUzmani.Models.ViewModels.Public;

public class GuideDetailViewModel
{
    public Guide Guide { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public List<Guide> RelatedGuides { get; set; } = new();
    public List<Category> AllCategories { get; set; } = new();
    public string Language { get; set; } = "tr";

    // SEO Properties
    public string Title => Language == "en" && !string.IsNullOrEmpty(Guide.TitleEn) 
        ? Guide.TitleEn 
        : Guide.TitleTr;

    public string MetaDescription => Language == "en" && !string.IsNullOrEmpty(Guide.MetaDescriptionEn) 
        ? Guide.MetaDescriptionEn 
        : Guide.MetaDescriptionTr ?? string.Empty;

    public string Keywords => Language == "en" && !string.IsNullOrEmpty(Guide.SeoKeywordsEn) 
        ? Guide.SeoKeywordsEn 
        : Guide.SeoKeywordsTr ?? string.Empty;

    public string Content => Language == "en" && !string.IsNullOrEmpty(Guide.ContentEn) 
        ? Guide.ContentEn 
        : Guide.ContentTr;

    public string Summary => Language == "en" && !string.IsNullOrEmpty(Guide.SummaryEn) 
        ? Guide.SummaryEn 
        : Guide.SummaryTr ?? string.Empty;
}



