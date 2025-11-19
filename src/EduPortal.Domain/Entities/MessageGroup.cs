using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace EduPortal.Domain.Entities;

public class MessageGroup : BaseAuditableEntity
{
    [Required]
    [MaxLength(200)]
    public string GroupName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public virtual ICollection<MessageGroupMember> Members { get; set; } = new List<MessageGroupMember>();
    public virtual ICollection<GroupMessage> Messages { get; set; } = new List<GroupMessage>();
}
