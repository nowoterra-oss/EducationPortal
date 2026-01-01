using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class Attendance : BaseAuditableEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int CourseId { get; set; }

    [Required]
    public int TeacherId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public AttendanceStatus Status { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    // Ders sonrası değerlendirme alanları
    [Range(0, 100)]
    public int? Performance { get; set; } // 0-100 performans puanı

    [MaxLength(2000)]
    public string? LessonEvaluation { get; set; } // Ders değerlendirme notu

    public bool IsEvaluationCompleted { get; set; } = false;

    // Ders programı bağlantısı (aynı gün birden fazla ders için)
    public int? ScheduleId { get; set; }

    [MaxLength(5)]
    public string? LessonTime { get; set; } // Format: "09:00"

    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    [ForeignKey(nameof(CourseId))]
    public virtual Course Course { get; set; } = null!;

    [ForeignKey(nameof(TeacherId))]
    public virtual Teacher Teacher { get; set; } = null!;
}
