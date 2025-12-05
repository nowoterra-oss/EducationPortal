using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

/// <summary>
/// Öğretmenin öğrenciye atadığı bireysel ödev kaydı
/// </summary>
public class HomeworkAssignment : BaseAuditableEntity
{
    [Required]
    public int HomeworkId { get; set; }

    [Required]
    public int StudentId { get; set; }

    [Required]
    public int TeacherId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    // Öğrenci ödevi gördü mü?
    public bool IsViewed { get; set; } = false;
    public DateTime? ViewedAt { get; set; }

    // Teslim durumu
    [Required]
    public HomeworkAssignmentStatus Status { get; set; } = HomeworkAssignmentStatus.Atandi;

    // Tamamlanma oranı (%10, %20, ... %100)
    [Range(0, 100)]
    public int CompletionPercentage { get; set; } = 0;

    // Öğretmen değerlendirmesi
    [MaxLength(2000)]
    public string? TeacherFeedback { get; set; }

    [Range(0, 100)]
    public int? Score { get; set; }

    public DateTime? GradedAt { get; set; }

    // Hatırlatma gönderildi mi?
    public bool ReminderSent { get; set; } = false;
    public DateTime? ReminderSentAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(HomeworkId))]
    public virtual Homework Homework { get; set; } = null!;

    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    [ForeignKey(nameof(TeacherId))]
    public virtual Teacher Teacher { get; set; } = null!;

    public virtual ICollection<HomeworkViewLog> ViewLogs { get; set; } = new List<HomeworkViewLog>();
}
