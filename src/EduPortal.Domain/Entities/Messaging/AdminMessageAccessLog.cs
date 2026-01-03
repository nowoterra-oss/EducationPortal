using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities.Messaging;

/// <summary>
/// Admin mesaj erişim logu - Admin'in başkalarının mesajlarını okuması kaydedilir
/// </summary>
public class AdminMessageAccessLog : BaseEntity
{
    /// <summary>
    /// Erişen admin kullanıcı ID
    /// </summary>
    [Required]
    public string AdminUserId { get; set; } = string.Empty;

    /// <summary>
    /// Erişilen konuşma ID
    /// </summary>
    [Required]
    public int ConversationId { get; set; }

    /// <summary>
    /// Erişilen mesaj ID (null ise tüm konuşmaya erişildi)
    /// </summary>
    public int? MessageId { get; set; }

    /// <summary>
    /// Erişim zamanı
    /// </summary>
    [Required]
    public DateTime AccessedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Erişim nedeni (zorunlu - neden erişildiğini açıklaması gerekir)
    /// </summary>
    [Required]
    [MaxLength(1000)]
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Admin'in IP adresi
    /// </summary>
    [MaxLength(50)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// Admin'in tarayıcı bilgisi
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Dekript edilmiş mesaj sayısı
    /// </summary>
    public int DecryptedMessageCount { get; set; } = 0;

    // Navigation Properties
    [ForeignKey(nameof(AdminUserId))]
    public virtual ApplicationUser AdminUser { get; set; } = null!;

    [ForeignKey(nameof(ConversationId))]
    public virtual Conversation Conversation { get; set; } = null!;

    [ForeignKey(nameof(MessageId))]
    public virtual ChatMessage? Message { get; set; }
}
