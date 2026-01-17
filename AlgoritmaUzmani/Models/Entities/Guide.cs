using System.ComponentModel.DataAnnotations;

namespace AlgoritmaUzmani.Models.Entities;

public class Guide
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    [Required]
    [MaxLength(300)]
    public string TitleTr { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? TitleEn { get; set; }

    [MaxLength(300)]
    public string SlugTr { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? SlugEn { get; set; }

    [MaxLength(500)]
    public string? SummaryTr { get; set; }

    [MaxLength(500)]
    public string? SummaryEn { get; set; }

    public string ContentTr { get; set; } = string.Empty;

    public string? ContentEn { get; set; }

    // SEO Fields
    [MaxLength(160)]
    public string? MetaDescriptionTr { get; set; }

    [MaxLength(160)]
    public string? MetaDescriptionEn { get; set; }

    [MaxLength(500)]
    public string? SeoKeywordsTr { get; set; }

    [MaxLength(500)]
    public string? SeoKeywordsEn { get; set; }

    // Featured Image
    [MaxLength(500)]
    public string? FeaturedImage { get; set; }

    [MaxLength(200)]
    public string? FeaturedImageAltTr { get; set; }

    [MaxLength(200)]
    public string? FeaturedImageAltEn { get; set; }

    public bool IsFeatured { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsTranslated { get; set; }

    public int ViewCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? PublishedAt { get; set; }

    // Navigation
    public Category Category { get; set; } = null!;
    public ICollection<GuideTag> GuideTags { get; set; } = new List<GuideTag>();
    public ICollection<GuideSeoTag> GuideSeoTags { get; set; } = new List<GuideSeoTag>();
    public ICollection<RelatedGuide> RelatedGuides { get; set; } = new List<RelatedGuide>();
    public ICollection<RelatedGuide> RelatedToGuides { get; set; } = new List<RelatedGuide>();
    public ICollection<CodeBlock> CodeBlocks { get; set; } = new List<CodeBlock>();
}



