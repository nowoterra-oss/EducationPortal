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

    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    [ForeignKey(nameof(CourseId))]
    public virtual Course Course { get; set; } = null!;

    [ForeignKey(nameof(TeacherId))]
    public virtual Teacher Teacher { get; set; } = null!;
}
