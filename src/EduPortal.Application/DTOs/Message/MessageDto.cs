namespace EduPortal.Application.DTOs.Message;

public class MessageDto
{
    public int Id { get; set; }

    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string? SenderEmail { get; set; }

    public string RecipientId { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string? RecipientEmail { get; set; }

    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;

    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? ReadAt { get; set; }

    public string? AttachmentUrl { get; set; }

    public int? ParentMessageId { get; set; }
    public int ReplyCount { get; set; }
}
