namespace EduPortal.Application.DTOs.Assessment;

public class CareerAssessmentDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentNo { get; set; } = string.Empty;
    public int CoachId { get; set; }
    public string CoachName { get; set; } = string.Empty;
    public DateTime AssessmentDate { get; set; }
    public string AssessmentType { get; set; } = string.Empty;
    public string? Results { get; set; }
    public string? Interpretation { get; set; }
    public string? RecommendedCareers { get; set; }
    public string? RecommendedFields { get; set; }
    public string? ReportUrl { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}
