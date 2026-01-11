using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AlgoritmaUzmani.Models.ViewModels.Admin;

public class GuideViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kategori seçilmelidir")]
    [Display(Name = "Kategori")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Türkçe başlık gereklidir")]
    [MaxLength(300)]
    [Display(Name = "Başlık (TR)")]
    public string TitleTr { get; set; } = string.Empty;

    [MaxLength(300)]
    [Display(Name = "Başlık (EN)")]
    public string? TitleEn { get; set; }

    [MaxLength(500)]
    [Display(Name = "Özet (TR)")]
    public string? SummaryTr { get; set; }

    [MaxLength(500)]
    [Display(Name = "Özet (EN)")]
    public string? SummaryEn { get; set; }

    [Required(ErrorMessage = "Türkçe içerik gereklidir")]
    [Display(Name = "İçerik (TR)")]
    public string ContentTr { get; set; } = string.Empty;

    [Display(Name = "İçerik (EN)")]
    public string? ContentEn { get; set; }

    // SEO
    [MaxLength(160)]
    [Display(Name = "Meta Açıklama (TR)")]
    public string? MetaDescriptionTr { get; set; }

    [MaxLength(160)]
    [Display(Name = "Meta Açıklama (EN)")]
    public string? MetaDescriptionEn { get; set; }

    [MaxLength(500)]
    [Display(Name = "SEO Anahtar Kelimeler (TR)")]
    public string? SeoKeywordsTr { get; set; }

    [MaxLength(500)]
    [Display(Name = "SEO Anahtar Kelimeler (EN)")]
    public string? SeoKeywordsEn { get; set; }

    // Image
    [Display(Name = "Öne Çıkan Görsel")]
    public string? FeaturedImage { get; set; }

    [MaxLength(200)]
    [Display(Name = "Görsel Alt Yazı (TR)")]
    public string? FeaturedImageAltTr { get; set; }

    [MaxLength(200)]
    [Display(Name = "Görsel Alt Yazı (EN)")]
    public string? FeaturedImageAltEn { get; set; }

    [Display(Name = "Öne Çıkan")]
    public bool IsFeatured { get; set; }

    [Display(Name = "Sıralama")]
    public int DisplayOrder { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Çevrildi")]
    public bool IsTranslated { get; set; }

    // Relations
    [Display(Name = "Etiketler")]
    public List<int> SelectedTagIds { get; set; } = new();

    [Display(Name = "SEO Etiketleri")]
    public List<int> SelectedSeoTagIds { get; set; } = new();

    [Display(Name = "İlgili Rehberler")]
    public List<int> RelatedGuideIds { get; set; } = new();

    // Select Lists
    public SelectList? Categories { get; set; }
    public SelectList? Tags { get; set; }
    public SelectList? SeoTags { get; set; }
    public SelectList? AllGuides { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int ViewCount { get; set; }
}

