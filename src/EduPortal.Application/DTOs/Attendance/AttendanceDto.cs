using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.Attendance;

public class AttendanceDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
