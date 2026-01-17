using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlgoritmaUzmani.Models.Entities;

public class CodeBlock
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int GuideId { get; set; }

    [ForeignKey(nameof(GuideId))]
    public Guide Guide { get; set; } = null!;

    /// <summary>
    /// Unique identifier for this code block within the guide
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string BlockId { get; set; } = string.Empty;

    /// <summary>
    /// Source programming language (e.g., "python", "javascript")
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string SourceLanguage { get; set; } = string.Empty;

    /// <summary>
    /// Source code in the original language
    /// </summary>
    [Required]
    public string SourceCode { get; set; } = string.Empty;

    /// <summary>
    /// JSON object containing translations: { "javascript": "code...", "java": "code..." }
    /// </summary>
    [Required]
    [Column(TypeName = "jsonb")]
    public string Translations { get; set; } = "{}";

    /// <summary>
    /// Display order within the guide
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Optional title/description for the code block
    /// </summary>
    [MaxLength(200)]
    public string? Title { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

