namespace EduPortal.Application.DTOs.SportsAssessment;

public class SportsAssessmentSummaryDto
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? CurrentSport { get; set; }
    public string? SkillLevel { get; set; }
    public DateTime AssessmentDate { get; set; }
}
