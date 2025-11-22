namespace EduPortal.Application.DTOs.Exam;

public class ExamStatisticsDto
{
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public decimal AverageScore { get; set; }
    public decimal HighestScore { get; set; }
    public decimal LowestScore { get; set; }
    public decimal PassRate { get; set; }
    public decimal MedianScore { get; set; }
}
