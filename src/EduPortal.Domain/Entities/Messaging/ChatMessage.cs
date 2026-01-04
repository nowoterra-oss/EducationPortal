using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities.Messaging;

/// <summary>
/// Sohbet mesajı entity'si
/// </summary>
public class ChatMessage : BaseEntity
{
    /// <summary>
    /// Konuşma ID
    /// </summary>
    [Required]
    public int ConversationId { get; set; }

    /// <summary>
    /// Gönderen kullanıcı ID
    /// </summary>
    [Required]
    public string SenderId { get; set; } = string.Empty;

    /// <summary>
    /// Şifrelenmiş mesaj içeriği (AES-256)
    /// </summary>
    [Required]
    public string ContentEncrypted { get; set; } = string.Empty;

    /// <summary>
    /// Mesaj içeriği hash'i (SHA-256 - bütünlük kontrolü için)
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
    /// Düzenleme zamanı (null ise düzenlenmemiş)
    /// </summary>
    public DateTime? EditedAt { get; set; }

    /// <summary>
    /// Düzenlenmiş mi?
    /// </summary>
    public bool IsEdited { get; set; } = false;

    /// <summary>
    /// Silinme zamanı
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Silen kullanıcı ID
    /// </summary>
    public string? DeletedBy { get; set; }

    /// <summary>
    /// Yanıt verilen mesaj ID (reply özelliği için)
    /// </summary>
    public int? ReplyToMessageId { get; set; }

    /// <summary>
    /// Sistem mesajı mı? (katılma, ayrılma vb.)
    /// </summary>
    public bool IsSystemMessage { get; set; } = false;

    // Navigation Properties
    [ForeignKey(nameof(ConversationId))]
    public virtual Conversation Conversation { get; set; } = null!;

    [ForeignKey(nameof(SenderId))]
    public virtual ApplicationUser Sender { get; set; } = null!;

    [ForeignKey(nameof(ReplyToMessageId))]
    public virtual ChatMessage? ReplyToMessage { get; set; }

    public virtual ICollection<MessageReadReceipt> ReadReceipts { get; set; } = new List<MessageReadReceipt>();

    /// <summary>
    /// İletildi bilgileri (çift tik gri için)
    /// </summary>
    public virtual ICollection<MessageDeliveryReceipt> DeliveryReceipts { get; set; } = new List<MessageDeliveryReceipt>();
}
