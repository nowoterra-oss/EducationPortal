using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities.Messaging;

/// <summary>
/// Toplu mesaj entity'si - Admin'in herkese veya belirli gruplara gönderdiği mesajlar
/// </summary>
public class BroadcastMessage : BaseEntity
{
    /// <summary>
    /// Gönderen admin kullanıcı ID
    /// </summary>
    [Required]
    public string SenderId { get; set; } = string.Empty;

    /// <summary>
    /// Hedef kitle (Students, Teachers, Parents, All vb.)
    /// </summary>
    [Required]
    public BroadcastTargetAudience TargetAudience { get; set; }

    /// <summary>
    /// Mesaj başlığı
    /// </summary>
    [MaxLength(200)]
    public string? Title { get; set; }

    /// <summary>
    /// Şifrelenmiş mesaj içeriği (AES-256)
    /// </summary>
    [Required]
    public string ContentEncrypted { get; set; } = string.Empty;

    /// <summary>
    /// Mesaj içeriği hash'i (SHA-256)
    /// </summary>
    [Required]
    [MaxLength(64)]
    public string ContentHash { get; set; } = string.Empty;

    /// <summary>
    /// Gönderilme zamanı
    /// </summary>
    [Required]
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Son geçerlilik tarihi (null ise süresiz)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Öncelik seviyesi
    /// </summary>
    public Priority Priority { get; set; } = Priority.Normal;

    /// <summary>
    /// Alıcı sayısı
    /// </summary>
    public int RecipientCount { get; set; } = 0;

    /// <summary>
    /// Okuyan sayısı
    /// </summary>
    public int ReadCount { get; set; } = 0;

    // Navigation Properties
    [ForeignKey(nameof(SenderId))]
    public virtual ApplicationUser Sender { get; set; } = null!;

    public virtual ICollection<BroadcastMessageRecipient> Recipients { get; set; } = new List<BroadcastMessageRecipient>();
}
