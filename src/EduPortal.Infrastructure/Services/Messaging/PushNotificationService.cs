using EduPortal.Application.DTOs.Messaging;
using EduPortal.Application.Interfaces.Messaging;
using EduPortal.Domain.Entities.Messaging;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace EduPortal.Infrastructure.Services.Messaging;

/// <summary>
/// Web Push Notification servisi implementasyonu
/// </summary>
public class PushNotificationService : IPushNotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PushNotificationService> _logger;
    private readonly HttpClient _httpClient;

    // VAPID keys - appsettings.json'dan alinacak
    private readonly string _vapidPublicKey;
    private readonly string _vapidPrivateKey;
    private readonly string _vapidSubject;

    public PushNotificationService(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<PushNotificationService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("PushNotification");

        _vapidPublicKey = configuration["Messaging:VapidPublicKey"] ?? "";
        _vapidPrivateKey = configuration["Messaging:VapidPrivateKey"] ?? "";
        _vapidSubject = configuration["Messaging:VapidSubject"] ?? "mailto:admin@eduportal.com";
    }

    public async Task<PushSubscriptionDto> SubscribeAsync(CreatePushSubscriptionDto dto, string userId)
    {
        // Mevcut subscription'i kontrol et
        var existing = await _context.PushSubscriptions
            .FirstOrDefaultAsync(p => p.Endpoint == dto.Endpoint);

        if (existing != null)
        {
            // Ayni endpoint farkli kullanicida ise guncelle
            if (existing.UserId != userId)
            {
                existing.UserId = userId;
                existing.P256dh = dto.Keys.P256dh;
                existing.Auth = dto.Keys.Auth;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.IsActive = true;
                existing.FailedAttempts = 0;

                await _context.SaveChangesAsync();
            }

            return MapToDto(existing);
        }

        // Yeni subscription
        var subscription = new PushSubscription
        {
            UserId = userId,
            Endpoint = dto.Endpoint,
            P256dh = dto.Keys.P256dh,
            Auth = dto.Keys.Auth,
            SubscribedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.PushSubscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        return MapToDto(subscription);
    }

    public async Task UnsubscribeAsync(string endpoint, string userId)
    {
        var subscription = await _context.PushSubscriptions
            .FirstOrDefaultAsync(p => p.Endpoint == endpoint && p.UserId == userId);

        if (subscription != null)
        {
            _context.PushSubscriptions.Remove(subscription);
            await _context.SaveChangesAsync();
        }
    }

    public async Task UnsubscribeAllAsync(string userId)
    {
        var subscriptions = await _context.PushSubscriptions
            .Where(p => p.UserId == userId)
            .ToListAsync();

        _context.PushSubscriptions.RemoveRange(subscriptions);
        await _context.SaveChangesAsync();
    }

    public async Task SendNotificationAsync(string userId, PushNotificationDto notification)
    {
        var subscriptions = await _context.PushSubscriptions
            .Where(p => p.UserId == userId && p.IsActive)
            .ToListAsync();

        foreach (var subscription in subscriptions)
        {
            await SendPushAsync(subscription, notification);
        }
    }

    public async Task SendNotificationToUsersAsync(IEnumerable<string> userIds, PushNotificationDto notification)
    {
        var subscriptions = await _context.PushSubscriptions
            .Where(p => userIds.Contains(p.UserId) && p.IsActive)
            .ToListAsync();

        var tasks = subscriptions.Select(s => SendPushAsync(s, notification));
        await Task.WhenAll(tasks);
    }

    public async Task SendNewMessageNotificationAsync(string recipientUserId, string senderName, string messagePreview, int conversationId)
    {
        var notification = new PushNotificationDto
        {
            Title = $"Yeni mesaj: {senderName}",
            Body = messagePreview,
            Icon = "/icons/message-icon.png",
            Badge = "/icons/badge.png",
            Url = $"/messages/{conversationId}",
            Tag = $"message_{conversationId}",
            Data = new Dictionary<string, string>
            {
                { "type", "new_message" },
                { "conversationId", conversationId.ToString() }
            }
        };

        await SendNotificationAsync(recipientUserId, notification);
    }

    public async Task SendBroadcastNotificationAsync(IEnumerable<string> recipientUserIds, string title, int broadcastId)
    {
        var notification = new PushNotificationDto
        {
            Title = "Duyuru",
            Body = title,
            Icon = "/icons/announcement-icon.png",
            Badge = "/icons/badge.png",
            Url = $"/broadcasts/{broadcastId}",
            Tag = $"broadcast_{broadcastId}",
            Data = new Dictionary<string, string>
            {
                { "type", "broadcast" },
                { "broadcastId", broadcastId.ToString() }
            }
        };

        await SendNotificationToUsersAsync(recipientUserIds, notification);
    }

    public async Task<List<PushSubscriptionDto>> GetSubscriptionsAsync(string userId)
    {
        var subscriptions = await _context.PushSubscriptions
            .Where(p => p.UserId == userId && p.IsActive)
            .ToListAsync();

        return subscriptions.Select(MapToDto).ToList();
    }

    public async Task CleanupFailedSubscriptionsAsync()
    {
        // 5'ten fazla basarisiz deneme yapilan veya 30 gunden fazla kullanilmayan subscriptionlari sil
        var cutoffDate = DateTime.UtcNow.AddDays(-30);

        var failedSubscriptions = await _context.PushSubscriptions
            .Where(p => p.FailedAttempts > 5 || (p.LastUsedAt != null && p.LastUsedAt < cutoffDate))
            .ToListAsync();

        _context.PushSubscriptions.RemoveRange(failedSubscriptions);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cleaned up {Count} failed/inactive push subscriptions", failedSubscriptions.Count);
    }

    #region Private Helpers

    private async Task SendPushAsync(PushSubscription subscription, PushNotificationDto notification)
    {
        try
        {
            // Web Push API kullanarak bildirim gonder
            // Bu basit implementasyon - production icin WebPush kutuphanesi kullanilmali

            var payload = JsonSerializer.Serialize(new
            {
                notification.Title,
                notification.Body,
                notification.Icon,
                notification.Badge,
                notification.Url,
                notification.Tag,
                notification.Data
            });

            // VAPID imzali request olustur
            // Not: Gercek implementasyon icin Lib.Net.Http.WebPush veya WebPush kutuphanesi kullanilmali

            var request = new HttpRequestMessage(HttpMethod.Post, subscription.Endpoint)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            // TTL header (24 saat)
            request.Headers.Add("TTL", "86400");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                subscription.LastUsedAt = DateTime.UtcNow;
                subscription.FailedAttempts = 0;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Gone ||
                     response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Subscription artik gecerli degil
                subscription.IsActive = false;
            }
            else
            {
                subscription.FailedAttempts++;
                if (subscription.FailedAttempts > 5)
                {
                    subscription.IsActive = false;
                }
            }

            subscription.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Push notification gonderilemedi: {Endpoint}", subscription.Endpoint);
            subscription.FailedAttempts++;
            subscription.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private static PushSubscriptionDto MapToDto(PushSubscription subscription)
    {
        return new PushSubscriptionDto
        {
            Id = subscription.Id,
            Endpoint = subscription.Endpoint,
            P256dh = subscription.P256dh,
            Auth = subscription.Auth,
            SubscribedAt = subscription.SubscribedAt,
            LastUsedAt = subscription.LastUsedAt,
            UserAgent = subscription.UserAgent,
            IsActive = subscription.IsActive
        };
    }

    #endregion
}
