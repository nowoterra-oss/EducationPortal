namespace EduPortal.Application.DTOs.Homework;

public class HomeworkDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime DueDate { get; set; }
    public int? MaxScore { get; set; }
    public string? AttachmentUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalSubmissions { get; set; }
}
