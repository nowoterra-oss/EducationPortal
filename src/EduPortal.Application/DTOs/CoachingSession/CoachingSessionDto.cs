namespace EduPortal.Application.DTOs.CoachingSession;

public class CoachingSessionDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int CoachId { get; set; }
    public string CoachName { get; set; } = string.Empty;
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public string CoachingArea { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime SessionDate { get; set; }
    public int DurationMinutes { get; set; }
    public string SessionType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? SessionNotes { get; set; }
    public string? ActionItems { get; set; }
    public string? StudentFeedback { get; set; }
    public int? Rating { get; set; }
    public DateTime? NextSessionDate { get; set; }
    public string? AttachmentUrl { get; set; }
    public DateTime CreatedDate { get; set; }
}
