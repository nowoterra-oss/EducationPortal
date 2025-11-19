using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.Homework;

public class HomeworkSubmissionDto
{
    public int Id { get; set; }
    public int HomeworkId { get; set; }
    public string HomeworkTitle { get; set; } = string.Empty;
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? SubmissionUrl { get; set; }
    public string? Comment { get; set; }
    public DateTime? SubmissionDate { get; set; }
    public HomeworkStatus Status { get; set; }
    public int? Score { get; set; }
    public string? TeacherFeedback { get; set; }
    public DateTime? GradedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
