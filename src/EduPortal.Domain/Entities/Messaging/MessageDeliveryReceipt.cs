using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities.Messaging;

/// <summary>
/// Mesaj iletildi bilgisi entity'si (çift tik gri için)
/// </summary>
public class MessageDeliveryReceipt : BaseEntity
{
    /// <summary>
    /// Mesaj ID
    /// </summary>
    [Required]
    public int MessageId { get; set; }

    /// <summary>
    /// Mesajın iletildiği kullanıcı ID
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// İletilme zamanı
    /// </summary>
    [Required]
    public DateTime DeliveredAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey(nameof(MessageId))]
    public virtual ChatMessage Message { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; } = null!;
}
