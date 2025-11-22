namespace EduPortal.Application.DTOs.Dashboard;

public class RecentActivityDto
{
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
}
