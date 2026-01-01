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

    // Progress tracking için
    public DateTime? StartedAt { get; set; } // Öğrenci ödeve başladığında

    [Range(0, 100)]
    public int ProgressPercentage { get; set; } = 0; // 0-100

    // Curriculum bağlantısı
    public int? CurriculumId { get; set; }

    // Teslim durumu
    [Required]
    public HomeworkAssignmentStatus Status { get; set; } = HomeworkAssignmentStatus.Atandi;

    // Tamamlanma oranı (%10, %20, ... %100)
    [Range(0, 100)]
    public int CompletionPercentage { get; set; } = 0;

    // Öğrenci teslimi
    [MaxLength(4000)]
    public string? SubmissionText { get; set; }

    [MaxLength(500)]
    public string? SubmissionUrl { get; set; }

    public DateTime? SubmittedAt { get; set; }

    // Öğrenci test teslimi
    [MaxLength(4000)]
    public string? TestSubmissionText { get; set; }

    [MaxLength(500)]
    public string? TestSubmissionUrl { get; set; }

    public DateTime? TestSubmittedAt { get; set; }

    // Öğretmen değerlendirmesi
    [MaxLength(2000)]
    public string? TeacherFeedback { get; set; }

    [Range(0, 100)]
    public int? Score { get; set; }

    public DateTime? GradedAt { get; set; }

    // Ayrı değerlendirmeler (ödev ve test için)
    [Range(0, 100)]
    public int? HomeworkScore { get; set; }

    [MaxLength(2000)]
    public string? HomeworkFeedback { get; set; }

    [Range(0, 100)]
    public int? TestScore { get; set; }

    [MaxLength(2000)]
    public string? TestFeedback { get; set; }

    // Hatırlatma gönderildi mi?
    public bool ReminderSent { get; set; } = false;
    public DateTime? ReminderSentAt { get; set; }

    // Ders İçeriği (JSON olarak saklanacak)
    [Column(TypeName = "nvarchar(max)")]
    public string? ContentResourcesJson { get; set; }

    // Ders Sonu Testi (JSON olarak saklanacak)
    [Column(TypeName = "nvarchar(max)")]
    public string? TestInfoJson { get; set; }

    public DateTime? TestDueDate { get; set; } // Test teslim tarihi

    /// <summary>
    /// Bu ödevde ders sonu testi var mı?
    /// </summary>
    public bool HasTest { get; set; } = false;

    // Navigation properties
    [ForeignKey(nameof(HomeworkId))]
    public virtual Homework Homework { get; set; } = null!;

    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    [ForeignKey(nameof(TeacherId))]
    public virtual Teacher Teacher { get; set; } = null!;

    [ForeignKey(nameof(CurriculumId))]
    public virtual Curriculum? Curriculum { get; set; }

    public virtual ICollection<HomeworkViewLog> ViewLogs { get; set; } = new List<HomeworkViewLog>();
    public virtual ICollection<HomeworkSubmissionFile> SubmissionFiles { get; set; } = new List<HomeworkSubmissionFile>();
    public virtual ICollection<HomeworkAttachment> Attachments { get; set; } = new List<HomeworkAttachment>();
}
