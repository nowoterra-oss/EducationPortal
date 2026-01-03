using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities.Messaging;

/// <summary>
/// Konuşma entity'si - Birebir veya grup sohbetleri için
/// </summary>
public class Conversation : BaseEntity
{
    /// <summary>
    /// Konuşma türü (Direct, CourseGroup, StudentGroup)
    /// </summary>
    [Required]
    public ConversationType Type { get; set; }

    /// <summary>
    /// Grup konuşmaları için başlık
    /// </summary>
    [MaxLength(200)]
    public string? Title { get; set; }

    /// <summary>
    /// Ders bazlı sohbet için Course ID
    /// </summary>
    public int? CourseId { get; set; }

    /// <summary>
    /// Öğrenci grubu bazlı sohbet için StudentGroup ID
    /// </summary>
    public int? StudentGroupId { get; set; }

    /// <summary>
    /// Konuşmadaki son mesajın ID'si (hızlı erişim için)
    /// </summary>
    public int? LastMessageId { get; set; }

    /// <summary>
    /// Son mesaj zamanı (sıralama için)
    /// </summary>
    public DateTime? LastMessageAt { get; set; }

    /// <summary>
    /// Maksimum katılımcı sayısı (grup sohbetleri için)
    /// </summary>
    public int MaxParticipants { get; set; } = 10;

    /// <summary>
    /// Şifreleme için kullanılan anahtar (her konuşma için benzersiz)
    /// </summary>
    [MaxLength(500)]
    public string? EncryptionKeyHash { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(CourseId))]
    public virtual Course? Course { get; set; }

    [ForeignKey(nameof(StudentGroupId))]
    public virtual StudentGroup? StudentGroup { get; set; }

    public virtual ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
    public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
