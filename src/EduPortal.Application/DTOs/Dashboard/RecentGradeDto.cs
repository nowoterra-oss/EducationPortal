namespace EduPortal.Application.DTOs.Dashboard;

public class RecentGradeDto
{
    public int GradeId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string AssessmentType { get; set; } = string.Empty;
    public string AssessmentTitle { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }
    public decimal Percentage { get; set; }
    public DateTime GradedDate { get; set; }
    public string? TeacherFeedback { get; set; }
}
