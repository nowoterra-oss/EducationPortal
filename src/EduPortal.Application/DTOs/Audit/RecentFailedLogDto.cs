namespace EduPortal.Application.DTOs.Audit;

public class RecentFailedLogDto
{
    public long Id { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; }
}
