using System.ComponentModel.DataAnnotations;

namespace AlgoritmaUzmani.Models.Entities;

public class StaticPage
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Slug { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string TitleTr { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? TitleEn { get; set; }

    public string ContentTr { get; set; } = string.Empty;

    public string? ContentEn { get; set; }

    [MaxLength(160)]
    public string? MetaDescriptionTr { get; set; }

    [MaxLength(160)]
    public string? MetaDescriptionEn { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}



