using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class WeeklySchedule : BaseAuditableEntity
{
    [Required]
    public int ClassId { get; set; }

    [Required]
    public int CourseId { get; set; }

    [Required]
    public int TeacherId { get; set; }

    public int? ClassroomId { get; set; }

    [Required]
    public int AcademicTermId { get; set; }

    [Required]
    public DayOfWeek DayOfWeek { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    [Required]
    [MaxLength(20)]
    public string AcademicYear { get; set; } = string.Empty; // "2024-2025"

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Notes { get; set; }

    [ForeignKey(nameof(ClassId))]
    public virtual Class Class { get; set; } = null!;

    [ForeignKey(nameof(CourseId))]
    public virtual Course Course { get; set; } = null!;

    [ForeignKey(nameof(TeacherId))]
    public virtual Teacher Teacher { get; set; } = null!;

    [ForeignKey(nameof(ClassroomId))]
    public virtual Classroom? Classroom { get; set; }

    [ForeignKey(nameof(AcademicTermId))]
    public virtual AcademicTerm AcademicTerm { get; set; } = null!;
}
