using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

/// <summary>
/// Ödev teslim dosyaları
/// </summary>
public class HomeworkSubmissionFile : BaseAuditableEntity
{
    [Required]
    public int HomeworkAssignmentId { get; set; }

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string FileUrl { get; set; } = string.Empty;

    public long FileSize { get; set; }

    [MaxLength(100)]
    public string? ContentType { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey(nameof(HomeworkAssignmentId))]
    public virtual HomeworkAssignment HomeworkAssignment { get; set; } = null!;
}
