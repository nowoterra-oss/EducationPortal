using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities.Messaging;

/// <summary>
/// Konuşma katılımcısı entity'si
/// </summary>
public class ConversationParticipant : BaseEntity
{
    /// <summary>
    /// Konuşma ID
    /// </summary>
    [Required]
    public int ConversationId { get; set; }

    /// <summary>
    /// Kullanıcı ID
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Katılımcı rolü (Owner, Participant, Admin)
    /// </summary>
    [Required]
    public ConversationParticipantRole Role { get; set; } = ConversationParticipantRole.Participant;

    /// <summary>
    /// Katılma tarihi
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Ayrılma tarihi (null ise hala aktif)
    /// </summary>
    public DateTime? LeftAt { get; set; }

    /// <summary>
    /// Son okunan mesaj ID (okundu bilgisi için)
    /// </summary>
    public int? LastReadMessageId { get; set; }

    /// <summary>
    /// Son okuma zamanı
    /// </summary>
    public DateTime? LastReadAt { get; set; }

    /// <summary>
    /// Şu anda yazıyor mu?
    /// </summary>
    public bool IsTyping { get; set; } = false;

    /// <summary>
    /// Son yazma zamanı (yazıyor göstergesi için timeout)
    /// </summary>
    public DateTime? LastTypingAt { get; set; }

    /// <summary>
    /// Bildirimler kapalı mı?
    /// </summary>
    public bool IsMuted { get; set; } = false;

    /// <summary>
    /// Konuşmayı sabitlemiş mi?
    /// </summary>
    public bool IsPinned { get; set; } = false;

    // Navigation Properties
    [ForeignKey(nameof(ConversationId))]
    public virtual Conversation Conversation { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; } = null!;

    [ForeignKey(nameof(LastReadMessageId))]
    public virtual ChatMessage? LastReadMessage { get; set; }
}
