namespace EduPortal.Application.DTOs.Coach;

public class CoachSummaryDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public List<string> CoachingAreas { get; set; } = new List<string>();
    public bool IsAvailable { get; set; }
    public int ActiveStudentCount { get; set; }
    public decimal AverageRating { get; set; }
}
