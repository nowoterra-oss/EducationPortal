using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class AGPMilestone : BaseEntity
{
    [Required]
    public int AGPId { get; set; }

    [Required]
    [Range(1, 12)]
    public int Month { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public MilestoneStatus Status { get; set; }

    [Range(0, 100)]
    public int CompletionPercentage { get; set; } = 0;

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [ForeignKey(nameof(AGPId))]
    public virtual AcademicDevelopmentPlan AGP { get; set; } = null!;
}
