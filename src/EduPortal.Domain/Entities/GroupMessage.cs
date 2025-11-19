using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class GroupMessage : BaseEntity
{
    [Required]
    public int GroupId { get; set; }

    [Required]
    public string SenderId { get; set; } = string.Empty;

    [Required]
    [MaxLength(5000)]
    public string Body { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? AttachmentUrl { get; set; }

    [Required]
    public DateTime SentAt { get; set; }

    [ForeignKey(nameof(GroupId))]
    public virtual MessageGroup Group { get; set; } = null!;

    [ForeignKey(nameof(SenderId))]
    public virtual ApplicationUser Sender { get; set; } = null!;
}
