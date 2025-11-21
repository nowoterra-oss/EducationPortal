using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class CareerAssessment : BaseAuditableEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int CoachId { get; set; }

    [Required]
    public DateTime AssessmentDate { get; set; }

    [Required]
    [MaxLength(200)]
    public string AssessmentType { get; set; } = string.Empty;

    [MaxLength(3000)]
    public string? Results { get; set; }

    [MaxLength(2000)]
    public string? Interpretation { get; set; }

    [MaxLength(1000)]
    public string? RecommendedCareers { get; set; }

    [MaxLength(1000)]
    public string? RecommendedFields { get; set; }

    [MaxLength(500)]
    public string? ReportUrl { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    [ForeignKey(nameof(CoachId))]
    public virtual Coach Coach { get; set; } = null!;
}
