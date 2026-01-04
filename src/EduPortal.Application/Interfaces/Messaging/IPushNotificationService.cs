using EduPortal.Application.DTOs.Messaging;

namespace EduPortal.Application.Interfaces.Messaging;

/// <summary>
/// Web Push Notification servisi
/// </summary>
public interface IPushNotificationService
{
    /// <summary>
    /// Push subscription kaydeder
    /// </summary>
    Task<PushSubscriptionDto> SubscribeAsync(CreatePushSubscriptionDto dto, string userId);

    /// <summary>
    /// Push subscription siler
    /// </summary>
    Task UnsubscribeAsync(string endpoint, string userId);

    /// <summary>
    /// Kullanicinin tum subscriptionlarini siler
    /// </summary>
    Task UnsubscribeAllAsync(string userId);

    /// <summary>
    /// Kullaniciya push notification gonderir
    /// </summary>
    Task SendNotificationAsync(string userId, PushNotificationDto notification);

    /// <summary>
    /// Birden fazla kullaniciya push notification gonderir
    /// </summary>
    Task SendNotificationToUsersAsync(IEnumerable<string> userIds, PushNotificationDto notification);

    /// <summary>
    /// Yeni mesaj bildirimi gonderir
    /// </summary>
    Task SendNewMessageNotificationAsync(string recipientUserId, string senderName, string messagePreview, int conversationId);

    /// <summary>
    /// Toplu mesaj bildirimi gonderir
    /// </summary>
    Task SendBroadcastNotificationAsync(IEnumerable<string> recipientUserIds, string title, int broadcastId);

    /// <summary>
    /// Kullanicinin subscriptionlarini getirir
    /// </summary>
    Task<List<PushSubscriptionDto>> GetSubscriptionsAsync(string userId);

    /// <summary>
    /// Basarisiz subscriptionlari temizler
    /// </summary>
    Task CleanupFailedSubscriptionsAsync();
}
