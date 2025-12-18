using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class CourseResource : BaseAuditableEntity
{
    [Required]
    public int CourseId { get; set; }

    public int? CurriculumId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string ResourceType { get; set; } = string.Empty; // "PDF", "Video", "Link"

    [Required]
    [MaxLength(500)]
    public string ResourceUrl { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Sunucudaki dosya yolu (uploads/resources/xxx.pdf gibi)
    /// </summary>
    [MaxLength(500)]
    public string? FilePath { get; set; }

    /// <summary>
    /// Orijinal dosya adi
    /// </summary>
    [MaxLength(255)]
    public string? FileName { get; set; }

    /// <summary>
    /// Dosya boyutu (bytes)
    /// </summary>
    public long? FileSize { get; set; }

    /// <summary>
    /// Dosya MIME tipi (application/pdf, video/mp4 vb.)
    /// </summary>
    [MaxLength(100)]
    public string? MimeType { get; set; }

    public bool IsVisible { get; set; } = true;

    [ForeignKey(nameof(CourseId))]
    public virtual Course Course { get; set; } = null!;

    [ForeignKey(nameof(CurriculumId))]
    public virtual Curriculum? Curriculum { get; set; }
}
