using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class SportsAssessment : BaseAuditableEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int CounselorId { get; set; }

    [Required]
    public DateTime AssessmentDate { get; set; }

    [MaxLength(100)]
    public string? CurrentSport { get; set; }

    public int? YearsOfExperience { get; set; }

    [MaxLength(100)]
    public string? SkillLevel { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? Height { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? Weight { get; set; }

    [MaxLength(1000)]
    public string? PhysicalAttributes { get; set; }

    [MaxLength(2000)]
    public string? Strengths { get; set; }

    [MaxLength(2000)]
    public string? Weaknesses { get; set; }

    [MaxLength(1000)]
    public string? RecommendedSports { get; set; }

    [MaxLength(2000)]
    public string? DevelopmentPlan { get; set; }

    [MaxLength(500)]
    public string? ReportUrl { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    [ForeignKey(nameof(CounselorId))]
    public virtual Counselor Counselor { get; set; } = null!;
}
