using System.ComponentModel.DataAnnotations;

namespace AlgoritmaUzmani.Models.ViewModels.Admin;

public class TagViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Türkçe isim gereklidir")]
    [MaxLength(100)]
    [Display(Name = "İsim (TR)")]
    public string NameTr { get; set; } = string.Empty;

    [MaxLength(100)]
    [Display(Name = "İsim (EN)")]
    public string? NameEn { get; set; }

    [MaxLength(20)]
    [Display(Name = "Renk")]
    public string? Color { get; set; }
}

public class SeoTagViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Türkçe isim gereklidir")]
    [MaxLength(100)]
    [Display(Name = "İsim (TR)")]
    public string NameTr { get; set; } = string.Empty;

    [MaxLength(100)]
    [Display(Name = "İsim (EN)")]
    public string? NameEn { get; set; }
}



