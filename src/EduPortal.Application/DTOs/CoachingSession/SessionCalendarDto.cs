namespace EduPortal.Application.DTOs.CoachingSession;

public class SessionCalendarDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string CoachName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string BackgroundColor { get; set; } = "#3B82F6"; // Tailwind blue-500
}
