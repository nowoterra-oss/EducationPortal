using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities.Messaging;

/// <summary>
/// Arşivlenmiş mesajlar - 1 yıldan eski mesajlar buraya taşınır
/// </summary>
public class MessageArchive : BaseEntity
{
    /// <summary>
    /// Orijinal konuşma ID
    /// </summary>
    [Required]
    public int OriginalConversationId { get; set; }

    /// <summary>
    /// Orijinal mesaj ID
    /// </summary>
    [Required]
    public int OriginalMessageId { get; set; }

    /// <summary>
    /// Gönderen kullanıcı ID
    /// </summary>
    [Required]
    public string SenderId { get; set; } = string.Empty;

    /// <summary>
    /// Şifrelenmiş mesaj içeriği
    /// </summary>
    [Required]
    public string ContentEncrypted { get; set; } = string.Empty;

    /// <summary>
    /// Mesaj içeriği hash'i
    /// </summary>
    [Required]
    [MaxLength(64)]
    public string ContentHash { get; set; } = string.Empty;

    /// <summary>
    /// Orijinal gönderilme zamanı
    /// </summary>
    [Required]
    public DateTime OriginalSentAt { get; set; }

    /// <summary>
    /// Arşivlenme zamanı
    /// </summary>
    [Required]
    public DateTime ArchivedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Arşiv nedeni
    /// </summary>
    [MaxLength(200)]
    public string? ArchiveReason { get; set; } = "Auto-archived after 1 year";
}
