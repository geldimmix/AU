using System.ComponentModel.DataAnnotations;

namespace AlgoritmaUzmani.Models.Entities;

/// <summary>
/// Site ayarları (header scripts, meta tags, vb.)
/// </summary>
public class SiteSetting
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    public string? Value { get; set; }

    [MaxLength(200)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? Category { get; set; } // Scripts, Meta, General

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}

