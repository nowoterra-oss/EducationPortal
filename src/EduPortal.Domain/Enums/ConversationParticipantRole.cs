namespace EduPortal.Domain.Enums;

/// <summary>
/// Konuşma katılımcısı rolü
/// </summary>
public enum ConversationParticipantRole
{
    /// <summary>
    /// Konuşmayı başlatan
    /// </summary>
    Owner = 1,

    /// <summary>
    /// Normal katılımcı
    /// </summary>
    Participant = 2,

    /// <summary>
    /// Grup yöneticisi (öğretmen vb.)
    /// </summary>
    Admin = 3
}
