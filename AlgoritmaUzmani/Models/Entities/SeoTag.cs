using System.ComponentModel.DataAnnotations;

namespace AlgoritmaUzmani.Models.Entities;

/// <summary>
/// Google botlarına yönelik SEO etiketleri
/// </summary>
public class SeoTag
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string NameTr { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? NameEn { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<GuideSeoTag> GuideSeoTags { get; set; } = new List<GuideSeoTag>();
}

