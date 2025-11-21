namespace EduPortal.Application.DTOs.Coach;

public class CoachPerformanceDto
{
    public int CoachId { get; set; }
    public string CoachName { get; set; } = string.Empty;

    // Session Statistics
    public int TotalSessionsScheduled { get; set; }
    public int TotalSessionsCompleted { get; set; }
    public int TotalSessionsCancelled { get; set; }
    public int TotalSessionsNoShow { get; set; }
    public decimal CompletionRate { get; set; }

    // Student Statistics
    public int TotalStudents { get; set; }
    public int ActiveStudents { get; set; }

    // Rating Statistics
    public decimal AverageRating { get; set; }
    public int TotalRatings { get; set; }

    // Time Period
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Revenue (optional)
    public decimal TotalRevenue { get; set; }
}
