using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.Messaging;

public class ConversationDto
{
    public int Id { get; set; }
    public ConversationType Type { get; set; }
    public string? Title { get; set; }
    public string DisplayName { get; set; } = string.Empty; // For direct chats: other person's name
    public string? DisplayPhoto { get; set; } // For direct chats: other person's photo
    public int? CourseId { get; set; }
    public string? CourseName { get; set; }
    public int? StudentGroupId { get; set; }
    public string? StudentGroupName { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int MaxParticipants { get; set; }
    public DateTime CreatedAt { get; set; }

    // Last message preview
    public ChatMessageDto? LastMessage { get; set; }

    // Current user's status in this conversation
    public int UnreadCount { get; set; }
    public bool IsMuted { get; set; }
    public bool IsPinned { get; set; }

    // Participants info
    public List<ConversationParticipantDto> Participants { get; set; } = new();
}

public class ConversationListDto
{
    public int Id { get; set; }
    public ConversationType Type { get; set; }
    public string? Title { get; set; }
    public string DisplayName { get; set; } = string.Empty; // For direct chats: other person's name
    public string? DisplayPhoto { get; set; } // For direct chats: other person's photo
    public List<string> ParticipantPhotos { get; set; } = new(); // For group chats: participant photos
    public DateTime? LastMessageAt { get; set; }
    public string? LastMessagePreview { get; set; }
    public string? LastMessageSenderName { get; set; }
    public int UnreadCount { get; set; }
    public bool IsMuted { get; set; }
    public bool IsPinned { get; set; }
    public bool IsTyping { get; set; }
    public string? TypingUserName { get; set; }
    public int ParticipantCount { get; set; }
}

public class CreateConversationDto
{
    public ConversationType Type { get; set; }
    public string? Title { get; set; }
    public int? CourseId { get; set; }
    public int? StudentGroupId { get; set; }
    public List<string> ParticipantUserIds { get; set; } = new();
}

public class ConversationParticipantDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? UserPhoto { get; set; }

    // Frontend "photo" ve "photoUrl" bekliyor - alias olarak sunuyoruz
    public string? Photo => UserPhoto;
    public string? PhotoUrl => UserPhoto;

    public string? UserRole { get; set; } // Student, Teacher, Parent, Admin, Counselor
    public ConversationParticipantRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LastReadAt { get; set; }
    public bool IsTyping { get; set; }
    public bool IsOnline { get; set; }
}
