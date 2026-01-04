namespace EduPortal.Application.Interfaces.Messaging;

/// <summary>
/// Mesajlasma yetkilendirme servisi - kim kime mesaj atabilir
/// </summary>
public interface IMessagingAuthorizationService
{
    /// <summary>
    /// Kullanici belirtilen kisiye mesaj atabilir mi?
    /// </summary>
    /// <param name="senderUserId">Gonderen kullanici ID</param>
    /// <param name="recipientUserId">Alici kullanici ID</param>
    /// <returns>Yetki sonucu ve varsa neden</returns>
    Task<(bool canMessage, string? reason)> CanMessageUserAsync(string senderUserId, string recipientUserId);

    /// <summary>
    /// Kullanici belirtilen konusmaya mesaj atabilir mi?
    /// </summary>
    Task<(bool canMessage, string? reason)> CanMessageInConversationAsync(string userId, int conversationId);

    /// <summary>
    /// Kullanici belirtilen gruba mesaj atabilir mi?
    /// </summary>
    Task<(bool canMessage, string? reason)> CanMessageGroupAsync(string userId, int groupId);

    /// <summary>
    /// Kullanici broadcast mesaj gonderebilir mi?
    /// </summary>
    Task<(bool canBroadcast, string? reason)> CanSendBroadcastAsync(string userId);

    /// <summary>
    /// Kullanicinin mesaj atabileceği kisilerin listesi
    /// </summary>
    Task<List<string>> GetAllowedRecipientsAsync(string userId);

    /// <summary>
    /// Kullanicinin mesaj atabileceği gruplarin listesi
    /// </summary>
    Task<List<int>> GetAllowedGroupsAsync(string userId);
}
