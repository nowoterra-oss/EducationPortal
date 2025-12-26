namespace EduPortal.Application.DTOs.SportsAssessment;

public class SportsAssessmentDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentNo { get; set; } = string.Empty;
    public int CounselorId { get; set; }
    public string CounselorName { get; set; } = string.Empty;
    public DateTime AssessmentDate { get; set; }
    public string? CurrentSport { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? SkillLevel { get; set; }
    public decimal? Height { get; set; }
    public decimal? Weight { get; set; }
    public string? PhysicalAttributes { get; set; }
    public string? Strengths { get; set; }
    public string? Weaknesses { get; set; }
    public string? RecommendedSports { get; set; }
    public string? DevelopmentPlan { get; set; }
    public string? ReportUrl { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}
