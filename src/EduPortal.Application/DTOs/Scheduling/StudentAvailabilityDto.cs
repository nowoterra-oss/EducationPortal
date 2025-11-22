namespace EduPortal.Application.DTOs.Scheduling;

public class StudentAvailabilityDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsRecurring { get; set; }
}

public class CreateStudentAvailabilityDto
{
    public int StudentId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int Type { get; set; } // AvailabilityType enum
    public string? Notes { get; set; }
    public bool IsRecurring { get; set; } = true;
}
