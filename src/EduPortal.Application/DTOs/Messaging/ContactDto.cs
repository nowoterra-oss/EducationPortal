namespace EduPortal.Application.DTOs.Messaging;

/// <summary>
/// Mesaj atabilecegim kisiler listesi
/// </summary>
public class ContactDto
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    // Frontend "FullName" bekliyor, alias olarak sunuyoruz
    public string FullName => Name;

    public string? Photo { get; set; }

    // Frontend "PhotoUrl" bekliyor, alias olarak sunuyoruz
    public string? PhotoUrl => Photo;

    public string Role { get; set; } = string.Empty; // Student, Teacher, Parent, Admin, Counselor
    public string? RoleDescription { get; set; } // e.g., "Matematik Ogretmeni", "Danisman"
    public string? Email { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastSeenAt { get; set; }

    // Context info
    public string? RelationshipContext { get; set; } // e.g., "Bireysel Ders", "Grup Dersi: Matematik A"
    public int? ExistingConversationId { get; set; } // If already have a conversation
}

public class ContactListDto
{
    public List<ContactDto> Teachers { get; set; } = new();
    public List<ContactDto> Students { get; set; } = new();
    public List<ContactDto> Parents { get; set; } = new();
    public List<ContactDto> Counselors { get; set; } = new();
    public List<ContactDto> Admins { get; set; } = new();
    public List<ContactGroupDto> Groups { get; set; } = new(); // Group chats
}

public class ContactGroupDto
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string GroupType { get; set; } = string.Empty; // "StudentGroup", "CourseGroup"
    public int MemberCount { get; set; }
    public int? ExistingConversationId { get; set; }
}
