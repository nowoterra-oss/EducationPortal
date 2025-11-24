namespace EduPortal.Application.DTOs.Message;

public class MessageSummaryDto
{
    public int Id { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string BodyPreview { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
    public bool HasAttachment { get; set; }
    public int ReplyCount { get; set; }
}
