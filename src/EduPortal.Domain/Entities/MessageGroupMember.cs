using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class MessageGroupMember : BaseEntity
{
    [Required]
    public int GroupId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public DateTime JoinedAt { get; set; }

    [ForeignKey(nameof(GroupId))]
    public virtual MessageGroup Group { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; } = null!;
}
