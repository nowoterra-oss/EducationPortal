using EduPortal.Application.DTOs.Messaging;
using EduPortal.Domain.Enums;

namespace EduPortal.Application.Interfaces.Messaging;

/// <summary>
/// Toplu mesaj servisi - Admin icin
/// </summary>
public interface IBroadcastService
{
    /// <summary>
    /// Toplu mesaj gonderir
    /// </summary>
    Task<BroadcastMessageDto> SendBroadcastAsync(CreateBroadcastMessageDto dto, string senderUserId);

    /// <summary>
    /// Belirli bir kullaniciya toplu mesaj gonderir
    /// </summary>
    Task<BroadcastMessageDto> SendDirectBroadcastAsync(string recipientUserId, string title, string content, string senderUserId);

    /// <summary>
    /// Kullanicinin aldigi toplu mesajlari listeler
    /// </summary>
    Task<List<BroadcastMessageListDto>> GetBroadcastMessagesAsync(string userId, int page = 1, int pageSize = 20);

    /// <summary>
    /// Toplu mesaj detayini getirir
    /// </summary>
    Task<BroadcastMessageDto?> GetBroadcastMessageAsync(int messageId, string userId);

    /// <summary>
    /// Toplu mesaji okundu olarak isaretler
    /// </summary>
    Task MarkBroadcastAsReadAsync(int messageId, string userId);

    /// <summary>
    /// Admin icin gonderilen toplu mesajlari listeler
    /// </summary>
    Task<List<BroadcastMessageListDto>> GetSentBroadcastsAsync(string adminUserId, int page = 1, int pageSize = 20);

    /// <summary>
    /// Hedef kitleye gore kullanici sayisini getirir
    /// </summary>
    Task<int> GetAudienceCountAsync(BroadcastTargetAudience audience);

    /// <summary>
    /// Okunmamis toplu mesaj sayisini getirir
    /// </summary>
    Task<int> GetUnreadBroadcastCountAsync(string userId);
}
