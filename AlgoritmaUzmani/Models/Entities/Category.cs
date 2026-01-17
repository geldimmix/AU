using System.ComponentModel.DataAnnotations;

namespace AlgoritmaUzmani.Models.Entities;

public class Category
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string NameTr { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? NameEn { get; set; }

    [MaxLength(200)]
    public string SlugTr { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? SlugEn { get; set; }

    [MaxLength(500)]
    public string? DescriptionTr { get; set; }

    [MaxLength(500)]
    public string? DescriptionEn { get; set; }

    [MaxLength(100)]
    public string? Icon { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<Guide> Guides { get; set; } = new List<Guide>();
}



