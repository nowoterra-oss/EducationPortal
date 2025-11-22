namespace EduPortal.Application.DTOs.Exam;

public class InternalExamDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string ExamType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime ExamDate { get; set; }
    public int? Duration { get; set; }
    public int MaxScore { get; set; }
    public string? Description { get; set; }
    public int TotalResults { get; set; }
    public DateTime CreatedAt { get; set; }
}
