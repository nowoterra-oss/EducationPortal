using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

/// <summary>
/// AGP Timeline dönem entity'si
/// </summary>
public class AgpPeriod : BaseEntity
{
    [Required]
    public int AgpId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [MaxLength(50)]
    public string? Color { get; set; }

    public int Order { get; set; }

    // Navigation properties
    [ForeignKey(nameof(AgpId))]
    public virtual AcademicDevelopmentPlan Agp { get; set; } = null!;

    public virtual ICollection<AgpTimelineMilestone> Milestones { get; set; } = new List<AgpTimelineMilestone>();

    public virtual ICollection<AgpActivity> Activities { get; set; } = new List<AgpActivity>();
}
