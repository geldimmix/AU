using System.ComponentModel.DataAnnotations;

namespace AlgoritmaUzmani.Models.ViewModels.Admin;

public class CategoryViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Türkçe isim gereklidir")]
    [MaxLength(200)]
    [Display(Name = "İsim (TR)")]
    public string NameTr { get; set; } = string.Empty;

    [MaxLength(200)]
    [Display(Name = "İsim (EN)")]
    public string? NameEn { get; set; }

    [MaxLength(500)]
    [Display(Name = "Açıklama (TR)")]
    public string? DescriptionTr { get; set; }

    [MaxLength(500)]
    [Display(Name = "Açıklama (EN)")]
    public string? DescriptionEn { get; set; }

    [MaxLength(100)]
    [Display(Name = "İkon")]
    public string? Icon { get; set; }

    [Display(Name = "Sıralama")]
    public int DisplayOrder { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    public int GuideCount { get; set; }
}

