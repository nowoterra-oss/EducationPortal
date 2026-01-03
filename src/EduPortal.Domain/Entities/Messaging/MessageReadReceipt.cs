using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities.Messaging;

/// <summary>
/// Mesaj okundu bilgisi entity'si
/// </summary>
public class MessageReadReceipt : BaseEntity
{
    /// <summary>
    /// Mesaj ID
    /// </summary>
    [Required]
    public int MessageId { get; set; }

    /// <summary>
    /// Okuyan kullanıcı ID
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Okunma zamanı
    /// </summary>
    [Required]
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey(nameof(MessageId))]
    public virtual ChatMessage Message { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; } = null!;
}
