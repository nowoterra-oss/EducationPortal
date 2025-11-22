using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.SportsAssessment;

public class UpdateSportsAssessmentDto
{
    [Required]
    public DateTime AssessmentDate { get; set; }

    [MaxLength(100)]
    public string? CurrentSport { get; set; }

    public int? YearsOfExperience { get; set; }

    [MaxLength(100)]
    public string? SkillLevel { get; set; }

    public decimal? Height { get; set; }

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
}
