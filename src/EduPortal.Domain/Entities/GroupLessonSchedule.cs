using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

/// <summary>
/// Grup Ders Programı - Grubun öğretmenle dersi
/// </summary>
public class GroupLessonSchedule : BaseAuditableEntity
{
    [Required]
    public int GroupId { get; set; }

    [Required]
    public int TeacherId { get; set; }

    [Required]
    public int CourseId { get; set; }

    [Required]
    public DayOfWeek DayOfWeek { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    [Required]
    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public bool IsRecurring { get; set; } = true;

    /// <summary>
    /// İptal edilen tarihler (virgülle ayrılmış)
    /// </summary>
    [MaxLength(2000)]
    public string? CancelledDates { get; set; }

    public LessonStatus Status { get; set; } = LessonStatus.Scheduled;

    public int? ClassroomId { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation
    [ForeignKey(nameof(GroupId))]
    public virtual StudentGroup Group { get; set; } = null!;

    [ForeignKey(nameof(TeacherId))]
    public virtual Teacher Teacher { get; set; } = null!;

    [ForeignKey(nameof(CourseId))]
    public virtual Course Course { get; set; } = null!;

    [ForeignKey(nameof(ClassroomId))]
    public virtual Classroom? Classroom { get; set; }
}
