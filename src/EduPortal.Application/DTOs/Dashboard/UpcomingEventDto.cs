namespace EduPortal.Application.DTOs.Dashboard;

public class UpcomingEventDto
{
    public int EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string? Description { get; set; }
}
