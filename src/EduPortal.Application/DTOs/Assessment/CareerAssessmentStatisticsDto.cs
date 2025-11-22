namespace EduPortal.Application.DTOs.Assessment;

public class CareerAssessmentStatisticsDto
{
    public int TotalAssessments { get; set; }
    public int AssessmentsThisMonth { get; set; }
    public int AssessmentsThisYear { get; set; }

    public Dictionary<string, int> AssessmentsByType { get; set; } = new();
    public Dictionary<string, int> TopCareerFields { get; set; } = new();
    public Dictionary<string, int> AssessmentsByCoach { get; set; } = new();
}
