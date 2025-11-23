namespace EduPortal.Application.DTOs.Audit;

public class CreateAuditLogDto
{
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public object? OldValues { get; set; }
    public object? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool IsSuccessful { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public string? AdditionalInfo { get; set; }
}
