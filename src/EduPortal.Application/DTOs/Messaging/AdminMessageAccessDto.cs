namespace EduPortal.Application.DTOs.Messaging;

/// <summary>
/// Admin icin mesaj erisim logu
/// </summary>
public class AdminMessageAccessLogDto
{
    public int Id { get; set; }
    public string AdminUserId { get; set; } = string.Empty;
    public string AdminUserName { get; set; } = string.Empty;
    public int ConversationId { get; set; }
    public string ConversationTitle { get; set; } = string.Empty;
    public List<string> ConversationParticipants { get; set; } = new();
    public int? MessageId { get; set; }
    public DateTime AccessedAt { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public int DecryptedMessageCount { get; set; }
}

/// <summary>
/// Admin konusma okuma istegi
/// </summary>
public class AdminReadConversationDto
{
    public int ConversationId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Admin icin konusma detayi (sifrelenmemis mesajlarla)
/// </summary>
public class AdminConversationDetailDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ConversationParticipantDto> Participants { get; set; } = new();
    public List<AdminChatMessageDto> Messages { get; set; } = new();
    public int TotalMessageCount { get; set; }
}

public class AdminChatMessageDto
{
    public int Id { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // Decrypted
    public DateTime SentAt { get; set; }
    public bool IsEdited { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
