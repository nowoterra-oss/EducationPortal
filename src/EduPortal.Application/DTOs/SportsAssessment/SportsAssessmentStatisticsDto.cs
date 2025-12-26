namespace EduPortal.Application.DTOs.SportsAssessment;

public class SportsAssessmentStatisticsDto
{
    public int TotalAssessments { get; set; }
    public int AssessmentsThisMonth { get; set; }
    public int AssessmentsThisYear { get; set; }

    public Dictionary<string, int> AssessmentsBySport { get; set; } = new();
    public Dictionary<string, int> AssessmentsBySkillLevel { get; set; } = new();
    public Dictionary<string, int> AssessmentsByCounselor { get; set; } = new();
}
