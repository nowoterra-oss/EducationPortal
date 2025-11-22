namespace EduPortal.Application.DTOs.Exam;

public class ExamResultDto
{
    public int Id { get; set; }
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public decimal Percentage { get; set; }
    public int? Rank { get; set; }
    public string? Notes { get; set; }
}
