using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities.Messaging;

/// <summary>
/// Web Push Notification subscription entity'si
/// </summary>
public class PushSubscription : BaseEntity
{
    /// <summary>
    /// Kullanıcı ID
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Push subscription endpoint URL
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// P256DH key (encryption için)
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string P256dh { get; set; } = string.Empty;

    /// <summary>
    /// Auth key
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Auth { get; set; } = string.Empty;

    /// <summary>
    /// Subscription oluşturulma tarihi
    /// </summary>
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Son kullanım tarihi
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Cihaz/tarayıcı bilgisi
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Başarısız gönderim sayısı (çok fazla olursa deaktif edilir)
    /// </summary>
    public int FailedAttempts { get; set; } = 0;

    // Navigation Properties
    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; } = null!;
}
