using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EduPortal.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ProfilePhotoUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual Student? Student { get; set; }
    public virtual Teacher? Teacher { get; set; }
    public virtual Counselor? Counselor { get; set; }
    public virtual ICollection<Parent> Parents { get; set; } = new List<Parent>();
    public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    public virtual ICollection<MessageGroupMember> MessageGroupMemberships { get; set; } = new List<MessageGroupMember>();
    public virtual ICollection<GroupMessage> GroupMessages { get; set; } = new List<GroupMessage>();
}
