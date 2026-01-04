using EduPortal.Application.DTOs.Messaging;

namespace EduPortal.Application.Interfaces.Messaging;

/// <summary>
/// Ana mesajlasma servisi
/// </summary>
public interface IMessagingService
{
    #region Conversations

    /// <summary>
    /// Kullanicinin konusmalarini listeler
    /// </summary>
    Task<List<ConversationListDto>> GetConversationsAsync(string userId, int page = 1, int pageSize = 20);

    /// <summary>
    /// Konusma detayini getirir
    /// </summary>
    Task<ConversationDto?> GetConversationAsync(int conversationId, string userId);

    /// <summary>
    /// Yeni konusma olusturur
    /// </summary>
    Task<ConversationDto> CreateConversationAsync(CreateConversationDto dto, string creatorUserId);

    /// <summary>
    /// Iki kullanici arasindaki mevcut konusmayi bulur veya yeni olusturur
    /// </summary>
    Task<ConversationDto> GetOrCreateDirectConversationAsync(string userId1, string userId2);

    /// <summary>
    /// Grup icin konusma olusturur veya mevcutu getirir
    /// </summary>
    Task<ConversationDto> GetOrCreateGroupConversationAsync(int groupId, string userId);

    /// <summary>
    /// Konusmayi siler (soft delete)
    /// </summary>
    Task DeleteConversationAsync(int conversationId, string userId);

    /// <summary>
    /// Konusmayi sessize alir/acar
    /// </summary>
    Task MuteConversationAsync(int conversationId, string userId, bool mute);

    /// <summary>
    /// Konusmayi sabitler/kaldirir
    /// </summary>
    Task PinConversationAsync(int conversationId, string userId, bool pin);

    #endregion

    #region Messages

    /// <summary>
    /// Konusmadaki mesajlari getirir
    /// </summary>
    Task<List<ChatMessageDto>> GetMessagesAsync(int conversationId, string userId, int page = 1, int pageSize = 50);

    /// <summary>
    /// Mesaj gonderir
    /// </summary>
    Task<ChatMessageDto> SendMessageAsync(SendMessageDto dto, string senderUserId);

    /// <summary>
    /// Mesaji duzenler
    /// </summary>
    Task<ChatMessageDto> EditMessageAsync(EditMessageDto dto, string userId);

    /// <summary>
    /// Mesaji siler
    /// </summary>
    Task DeleteMessageAsync(int messageId, string userId);

    /// <summary>
    /// Mesajin conversation id'sini getirir
    /// </summary>
    Task<int?> GetMessageConversationIdAsync(int messageId, string userId);

    /// <summary>
    /// Mesajlari okundu olarak isaretler
    /// </summary>
    Task MarkAsReadAsync(MarkAsReadDto dto, string userId);

    /// <summary>
    /// Kullaniciya henuz iletilmemis mesajlari getirir (gri cift tik icin)
    /// </summary>
    Task<List<ChatMessageDto>> GetUndeliveredMessagesForUserAsync(int conversationId, string userId);

    /// <summary>
    /// Mesaji iletildi olarak isaretler (gri cift tik)
    /// </summary>
    Task MarkMessageAsDeliveredAsync(int messageId, string userId);

    /// <summary>
    /// Kullanicinin henuz okumadigi mesajlari getirir (mavi tik icin)
    /// </summary>
    Task<List<ChatMessageDto>> GetUnreadMessagesForUserAsync(int conversationId, string userId);

    #endregion

    #region Typing Indicator

    /// <summary>
    /// Yaziyor gostergesini gunceller
    /// </summary>
    Task UpdateTypingStatusAsync(int conversationId, string userId, bool isTyping);

    /// <summary>
    /// Konusmada yazan kullanicilari getirir
    /// </summary>
    Task<List<TypingIndicatorDto>> GetTypingUsersAsync(int conversationId);

    #endregion

    #region Contacts

    /// <summary>
    /// Kullanicinin mesaj atabileceği kisileri getirir
    /// </summary>
    Task<ContactListDto> GetContactsAsync(string userId);

    /// <summary>
    /// Kisi/grup arar
    /// </summary>
    Task<ContactListDto> SearchContactsAsync(string userId, string searchTerm);

    #endregion

    #region Unread Count

    /// <summary>
    /// Toplam okunmamis mesaj sayisini getirir
    /// </summary>
    Task<int> GetTotalUnreadCountAsync(string userId);

    /// <summary>
    /// Konusma bazli okunmamis mesaj sayilarini getirir
    /// </summary>
    Task<Dictionary<int, int>> GetUnreadCountsAsync(string userId);

    #endregion
}
