using System.ComponentModel.DataAnnotations;

namespace AlgoritmaUzmani.Models.Entities;

/// <summary>
/// Kullanıcıya yönelik ana etiketler
/// </summary>
public class Tag
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string NameTr { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? NameEn { get; set; }

    [MaxLength(100)]
    public string SlugTr { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? SlugEn { get; set; }

    [MaxLength(20)]
    public string? Color { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<GuideTag> GuideTags { get; set; } = new List<GuideTag>();
}

