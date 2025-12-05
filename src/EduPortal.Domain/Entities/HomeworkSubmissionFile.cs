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
    public int SubmissionId { get; set; }

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string FileUrl { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string FileType { get; set; } = string.Empty; // pdf, doc, docx, etc.

    public long FileSizeBytes { get; set; }

    // Navigation property
    [ForeignKey(nameof(SubmissionId))]
    public virtual StudentHomeworkSubmission Submission { get; set; } = null!;
}
