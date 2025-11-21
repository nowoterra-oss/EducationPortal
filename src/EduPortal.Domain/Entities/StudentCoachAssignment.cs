using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class StudentCoachAssignment : BaseAuditableEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int CoachId { get; set; }

    [Required]
    public CoachingArea CoachingArea { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(1000)]
    public string? Goals { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    [ForeignKey(nameof(CoachId))]
    public virtual Coach Coach { get; set; } = null!;
}
