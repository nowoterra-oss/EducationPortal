namespace EduPortal.Application.DTOs.Dashboard;

public class TodayScheduleDto
{
    public int ScheduleId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? Classroom { get; set; }
    public int StudentCount { get; set; }
}
