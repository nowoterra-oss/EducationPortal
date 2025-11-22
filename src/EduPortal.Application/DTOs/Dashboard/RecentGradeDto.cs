namespace EduPortal.Application.DTOs.Dashboard;

public class RecentGradeDto
{
    public int ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }
    public DateTime ExamDate { get; set; }
}
