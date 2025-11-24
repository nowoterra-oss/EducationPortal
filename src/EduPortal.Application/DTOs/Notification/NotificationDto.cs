using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.Notification;

public class NotificationDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string TypeName => Type.ToString();
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? ActionUrl { get; set; }
    public string? ActionText { get; set; }
    public int? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public DateTime CreatedAt { get; set; }
}
