namespace EduPortal.Application.DTOs.Scheduling;

public class WeeklyCalendarDto
{
    public int EntityId { get; set; } // Student or Teacher ID
    public string EntityName { get; set; } = string.Empty;
    public Dictionary<DayOfWeek, List<TimeSlotDto>> Schedule { get; set; } = new();
}

public class TimeSlotDto
{
    public int? Id { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Type { get; set; } = string.Empty; // Available, Busy, Unavailable
    public string? Title { get; set; }
    public string? SubTitle { get; set; }
    public string Color { get; set; } = string.Empty; // Hex color
    public bool IsClickable { get; set; }
}

public class MatchingResultDto
{
    public List<TimeSlotDto> MatchingSlots { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}
