namespace EduPortal.Application.DTOs.Audit;

public class AuditLogFilterDto
{
    public string? UserId { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? Action { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsSuccessful { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
