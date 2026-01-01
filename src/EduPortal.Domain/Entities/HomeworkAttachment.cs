using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class HomeworkAttachment : BaseAuditableEntity
{
    [Required]
    public int HomeworkAssignmentId { get; set; }

    [Required]
    [MaxLength(500)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string FilePath { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? MimeType { get; set; }

    public long FileSize { get; set; }

    public bool IsFromCourseResource { get; set; } = false;

    public int? CourseResourceId { get; set; }

    // Navigation
    [ForeignKey(nameof(HomeworkAssignmentId))]
    public virtual HomeworkAssignment HomeworkAssignment { get; set; } = null!;

    [ForeignKey(nameof(CourseResourceId))]
    public virtual CourseResource? CourseResource { get; set; }
}
