using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

/// <summary>
/// Öğretmenin haftalık müsaitlik durumu
/// </summary>
public class TeacherAvailability : BaseAuditableEntity
{
    [Required]
    public int TeacherId { get; set; }

    [Required]
    public DayOfWeek DayOfWeek { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    [Required]
    public AvailabilityType Type { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public bool IsRecurring { get; set; } = true;

    public DateTime? SpecificDate { get; set; }

    // Navigation
    [ForeignKey(nameof(TeacherId))]
    public virtual Teacher Teacher { get; set; } = null!;
}
