using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities.Messaging;

/// <summary>
/// Toplu mesaj alıcısı - Her kullanıcı için okundu bilgisi
/// </summary>
public class BroadcastMessageRecipient : BaseEntity
{
    /// <summary>
    /// Broadcast mesaj ID
    /// </summary>
    [Required]
    public int BroadcastMessageId { get; set; }

    /// <summary>
    /// Alıcı kullanıcı ID
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Okundu mu?
    /// </summary>
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// Okunma zamanı
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// Silindi mi? (Kullanıcı kendi listesinden sildi)
    /// </summary>
    public bool IsDeletedByUser { get; set; } = false;

    // Navigation Properties
    [ForeignKey(nameof(BroadcastMessageId))]
    public virtual BroadcastMessage BroadcastMessage { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; } = null!;
}
