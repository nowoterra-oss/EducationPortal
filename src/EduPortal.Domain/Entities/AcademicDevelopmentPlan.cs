using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class AcademicDevelopmentPlan : BaseAuditableEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    [MaxLength(20)]
    public string AcademicYear { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [MaxLength(500)]
    public string? PlanDocumentUrl { get; set; }

    [Required]
    public AGPStatus Status { get; set; }

    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    public virtual ICollection<AGPMilestone> Milestones { get; set; } = new List<AGPMilestone>();

    // Timeline periods
    public virtual ICollection<AgpPeriod> Periods { get; set; } = new List<AgpPeriod>();
}
