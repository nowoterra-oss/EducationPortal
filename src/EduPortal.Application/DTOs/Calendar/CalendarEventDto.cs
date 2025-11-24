using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.Calendar;

public class CalendarEventDto
{
    public int Id { get; set; }
    public int? StudentId { get; set; }
    public string? StudentName { get; set; }
    public int? ClassId { get; set; }
    public string? ClassName { get; set; }
    public EventScope Scope { get; set; }
    public string ScopeName => Scope.ToString();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EventType EventType { get; set; }
    public string EventTypeName => EventType.ToString();
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool AllDayEvent { get; set; }
    public string? Location { get; set; }
    public bool IsCompleted { get; set; }
    public Priority Priority { get; set; }
    public string PriorityName => Priority.ToString();
    public DateTime? Reminder { get; set; }
    public DateTime CreatedAt { get; set; }
}
