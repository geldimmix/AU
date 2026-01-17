using System.ComponentModel.DataAnnotations;

namespace AlgoritmaUzmani.Models.Entities;

/// <summary>
/// Ziyaretçi log kaydı
/// </summary>
public class VisitorLog
{
    public long Id { get; set; }

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    [MaxLength(100)]
    public string? Browser { get; set; }

    [MaxLength(100)]
    public string? BrowserVersion { get; set; }

    [MaxLength(100)]
    public string? OperatingSystem { get; set; }

    [MaxLength(50)]
    public string? DeviceType { get; set; } // Desktop, Mobile, Tablet

    [MaxLength(500)]
    public string? PageUrl { get; set; }

    [MaxLength(200)]
    public string? PageTitle { get; set; }

    [MaxLength(1000)]
    public string? Referrer { get; set; }

    [MaxLength(10)]
    public string? Language { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    public DateTime VisitedAt { get; set; } = DateTime.UtcNow;

    // Session tracking
    [MaxLength(100)]
    public string? SessionId { get; set; }

    public bool IsNewVisitor { get; set; }
}



