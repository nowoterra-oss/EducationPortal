using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class StudentCurriculumProgress : BaseAuditableEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int CurriculumId { get; set; }

    public bool IsTopicCompleted { get; set; } = false;
    public DateTime? TopicCompletedAt { get; set; }

    public bool AreHomeworksCompleted { get; set; } = false;
    public DateTime? HomeworksCompletedAt { get; set; }

    public bool IsExamUnlocked { get; set; } = false;
    public DateTime? ExamUnlockedAt { get; set; }

    public bool IsExamCompleted { get; set; } = false;
    public DateTime? ExamCompletedAt { get; set; }

    [Range(0, 100)]
    public int? ExamScore { get; set; }

    // Öğretmen onayı
    public bool IsApprovedByTeacher { get; set; } = false;
    public int? ApprovedByTeacherId { get; set; }
    public DateTime? ApprovedAt { get; set; }

    // Navigation
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    [ForeignKey(nameof(CurriculumId))]
    public virtual Curriculum Curriculum { get; set; } = null!;

    [ForeignKey(nameof(ApprovedByTeacherId))]
    public virtual Teacher? ApprovedByTeacher { get; set; }
}
