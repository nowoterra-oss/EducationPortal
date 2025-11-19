using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class Message : BaseEntity
{
    [Required]
    public string SenderId { get; set; } = string.Empty;

    [Required]
    public string RecipientId { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Subject { get; set; }

    [Required]
    [MaxLength(5000)]
    public string Body { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;

    [MaxLength(500)]
    public string? AttachmentUrl { get; set; }

    public int? ParentMessageId { get; set; }

    [Required]
    public DateTime SentAt { get; set; }

    public DateTime? ReadAt { get; set; }

    [ForeignKey(nameof(SenderId))]
    public virtual ApplicationUser Sender { get; set; } = null!;

    [ForeignKey(nameof(RecipientId))]
    public virtual ApplicationUser Recipient { get; set; } = null!;

    [ForeignKey(nameof(ParentMessageId))]
    public virtual Message? ParentMessage { get; set; }

    public virtual ICollection<Message> Replies { get; set; } = new List<Message>();
}
