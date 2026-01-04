namespace EduPortal.Application.DTOs.Messaging;

public class ChatMessageDto
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string? SenderPhoto { get; set; }
    public string Content { get; set; } = string.Empty; // Decrypted content
    public DateTime SentAt { get; set; }
    public DateTime? EditedAt { get; set; }
    public bool IsEdited { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsSystemMessage { get; set; }

    // Reply info
    public int? ReplyToMessageId { get; set; }
    public ChatMessageReplyDto? ReplyToMessage { get; set; }

    // Read receipts (for the sender) - Mavi tik
    public List<MessageReadReceiptDto> ReadReceipts { get; set; } = new();

    // Okuyan kullanici id'leri listesi (frontend icin)
    public List<string> ReadByUserIds { get; set; } = new();

    // Delivery receipts (for the sender) - Gri çift tik
    public List<MessageDeliveryReceiptDto> DeliveryReceipts { get; set; } = new();

    // Mesajin iletildigi kullanici id'leri listesi (frontend icin)
    public List<string> DeliveredToUserIds { get; set; } = new();

    // Is this message from the current user?
    public bool IsMine { get; set; }

    // Has the current user read this message?
    public bool IsRead { get; set; }

    // Has the message been delivered to recipient(s)?
    public bool IsDelivered { get; set; }
}

public class ChatMessageReplyDto
{
    public int Id { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string ContentPreview { get; set; } = string.Empty; // First 100 chars
    public bool IsDeleted { get; set; }
}

public class SendMessageDto
{
    public int ConversationId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int? ReplyToMessageId { get; set; }
}

public class EditMessageDto
{
    public int MessageId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class MessageReadReceiptDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? UserPhoto { get; set; }
    public DateTime ReadAt { get; set; }
}

public class MessageDeliveryReceiptDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? UserPhoto { get; set; }
    public DateTime DeliveredAt { get; set; }
}

public class MarkAsReadDto
{
    public int ConversationId { get; set; }
    public int? MessageId { get; set; } // If null, mark all as read
}

public class TypingIndicatorDto
{
    public int ConversationId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public bool IsTyping { get; set; }
}
