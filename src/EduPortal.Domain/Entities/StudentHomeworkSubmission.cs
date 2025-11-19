using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class StudentHomeworkSubmission : BaseAuditableEntity
{
    [Required]
    public int HomeworkId { get; set; }

    [Required]
    public int StudentId { get; set; }

    public DateTime? SubmissionDate { get; set; }

    [MaxLength(500)]
    public string? SubmissionUrl { get; set; }

    [Range(0, 100)]
    public int CompletionPercentage { get; set; } = 0;

    [Column(TypeName = "decimal(5,2)")]
    public decimal? Score { get; set; }

    [MaxLength(2000)]
    public string? TeacherFeedback { get; set; }

    [Required]
    public HomeworkStatus Status { get; set; }

    [ForeignKey(nameof(HomeworkId))]
    public virtual Homework Homework { get; set; } = null!;

    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
}
