namespace EduPortal.Application.DTOs.Scheduling;

public class TeacherAvailabilityDto
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsRecurring { get; set; }
}

public class CreateTeacherAvailabilityDto
{
    public int TeacherId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int Type { get; set; }
    public string? Notes { get; set; }
    public bool IsRecurring { get; set; } = true;
}
