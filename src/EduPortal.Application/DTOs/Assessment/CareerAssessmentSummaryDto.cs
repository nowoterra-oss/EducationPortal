namespace EduPortal.Application.DTOs.Assessment;

public class CareerAssessmentSummaryDto
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string CoachName { get; set; } = string.Empty;
    public DateTime AssessmentDate { get; set; }
    public string AssessmentType { get; set; } = string.Empty;
    public List<string> TopCareerSuggestions { get; set; } = new();
}
