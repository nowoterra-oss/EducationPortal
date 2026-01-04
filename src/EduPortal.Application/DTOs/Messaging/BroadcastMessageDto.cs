using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.Messaging;

public class BroadcastMessageDto
{
    public int Id { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string? SenderPhoto { get; set; }
    public BroadcastTargetAudience TargetAudience { get; set; }
    public string TargetAudienceDisplay { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public Priority Priority { get; set; }
    public int RecipientCount { get; set; }
    public int ReadCount { get; set; }
    public bool IsRead { get; set; } // For current user
    public DateTime? ReadAt { get; set; } // When current user read it
}

public class CreateBroadcastMessageDto
{
    public BroadcastTargetAudience TargetAudience { get; set; }
    public string? Title { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public Priority Priority { get; set; } = Priority.Normal;
}

public class BroadcastMessageListDto
{
    public int Id { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string TargetAudienceDisplay { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string ContentPreview { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public Priority Priority { get; set; }
    public bool IsRead { get; set; }
    public int RecipientCount { get; set; }
    public int ReadCount { get; set; }
}
