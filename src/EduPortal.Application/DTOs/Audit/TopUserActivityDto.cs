namespace EduPortal.Application.DTOs.Audit;

public class TopUserActivityDto
{
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public int ActivityCount { get; set; }
    public DateTime LastActivity { get; set; }
}
