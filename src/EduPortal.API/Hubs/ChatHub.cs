using EduPortal.Application.DTOs.Messaging;
using EduPortal.Application.Interfaces.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace EduPortal.API.Hubs;

/// <summary>
/// Real-time mesajlasma hub'i
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly IMessagingService _messagingService;
    private readonly IMessagingAuthorizationService _authorizationService;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ILogger<ChatHub> _logger;

    // Kullanici baglanti takibi - thread-safe
    private static readonly ConcurrentDictionary<string, HashSet<string>> UserConnections = new();
    private static readonly object LockObject = new();

    public ChatHub(
        IMessagingService messagingService,
        IMessagingAuthorizationService authorizationService,
        IPushNotificationService pushNotificationService,
        ILogger<ChatHub> logger)
    {
        _messagingService = messagingService;
        _authorizationService = authorizationService;
        _pushNotificationService = pushNotificationService;
        _logger = logger;
    }

    private string UserId => Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    private string UserName => Context.User?.FindFirstValue(ClaimTypes.Name) ?? Context.User?.FindFirstValue("name") ?? "Kullanıcı";

    public override async Task OnConnectedAsync()
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId))
        {
            await base.OnConnectedAsync();
            return;
        }

        // Kullanici grubuna ekle
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

        // Baglanti takibi
        bool isFirstConnection;
        lock (LockObject)
        {
            if (!UserConnections.ContainsKey(userId))
            {
                UserConnections[userId] = new HashSet<string>();
            }
            isFirstConnection = UserConnections[userId].Count == 0;
            UserConnections[userId].Add(Context.ConnectionId);
        }

        // Ilk baglanti ise diger kullanicilara haber ver
        if (isFirstConnection)
        {
            await Clients.Others.SendAsync("UserOnline", userId);
        }

        _logger.LogInformation("User {UserId} connected with connection {ConnectionId}", userId, Context.ConnectionId);

        // Okunmamis mesaj sayisini gonder
        var unreadCount = await _messagingService.GetTotalUnreadCountAsync(userId);
        await Clients.Caller.SendAsync("UnreadCountUpdated", unreadCount);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = UserId;

        if (!string.IsNullOrEmpty(userId))
        {
            bool wasLastConnection = false;

            // Baglanti takibinden cikar
            lock (LockObject)
            {
                if (UserConnections.TryGetValue(userId, out var connections))
                {
                    connections.Remove(Context.ConnectionId);
                    if (connections.Count == 0)
                    {
                        UserConnections.TryRemove(userId, out _);
                        wasLastConnection = true;
                    }
                }
            }

            // Son baglanti ise diger kullanicilara haber ver
            if (wasLastConnection)
            {
                await Clients.Others.SendAsync("UserOffline", userId);
            }

            _logger.LogInformation("User {UserId} disconnected from connection {ConnectionId}", userId, Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Konusmaya katil (mesajlari almak icin)
    /// </summary>
    public async Task JoinConversation(int conversationId)
    {
        try
        {
            if (string.IsNullOrEmpty(UserId))
            {
                await Clients.Caller.SendAsync("Error", "Kullanıcı kimliği bulunamadı.");
                return;
            }

            var (canMessage, _) = await _authorizationService.CanMessageInConversationAsync(UserId, conversationId);
            if (!canMessage)
            {
                await Clients.Caller.SendAsync("Error", "Bu konuşmaya erişim yetkiniz yok.");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
            _logger.LogInformation("[JoinConversation] User {UserId} joined conversation {ConversationId}", UserId, conversationId);

            // Bu kullaniciya henuz iletilmemis mesajlari bul ve iletildi olarak isaretlesi (gri cift tik)
            var undeliveredMessages = await _messagingService.GetUndeliveredMessagesForUserAsync(conversationId, UserId);
            foreach (var message in undeliveredMessages)
            {
                await _messagingService.MarkMessageAsDeliveredAsync(message.Id, UserId);

                // Mesajin gonderenine iletildi bilgisi gonder
                await Clients.User(message.SenderId).SendAsync("MessageDelivered", conversationId, message.Id, UserId);
            }

            if (undeliveredMessages.Count > 0)
            {
                _logger.LogInformation("[JoinConversation] Marked {Count} messages as delivered for user {UserId} in conversation {ConversationId}",
                    undeliveredMessages.Count, UserId, conversationId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[JoinConversation] Error for conversation {ConversationId}", conversationId);
            await Clients.Caller.SendAsync("Error", "Konuşmaya katılırken bir hata oluştu.");
        }
    }

    /// <summary>
    /// Konusmadan ayril
    /// </summary>
    public async Task LeaveConversation(int conversationId)
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
            _logger.LogInformation("[LeaveConversation] User {UserId} left conversation {ConversationId}", UserId, conversationId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[LeaveConversation] Error for conversation {ConversationId}", conversationId);
        }
    }

    /// <summary>
    /// Mesaj gonder
    /// </summary>
    public async Task SendMessage(int conversationId, string content, int? replyToMessageId = null)
    {
        _logger.LogInformation("[SendMessage] Started: ConversationId={ConversationId}, UserId={UserId}, ContentLength={ContentLength}",
            conversationId, UserId, content?.Length ?? 0);

        try
        {
            if (string.IsNullOrEmpty(UserId))
            {
                _logger.LogWarning("[SendMessage] UserId is null or empty");
                await Clients.Caller.SendAsync("Error", "Kullanıcı kimliği bulunamadı.");
                return;
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("[SendMessage] Content is null or empty");
                await Clients.Caller.SendAsync("Error", "Mesaj içeriği boş olamaz.");
                return;
            }

            _logger.LogInformation("[SendMessage] Calling SendMessageAsync...");
            var message = await _messagingService.SendMessageAsync(new SendMessageDto
            {
                ConversationId = conversationId,
                Content = content,
                ReplyToMessageId = replyToMessageId
            }, UserId);

            _logger.LogInformation("[SendMessage] Message saved with Id={MessageId}", message.Id);

            // Konusmadaki herkese mesaji gonder - Frontend "ReceiveMessage" bekliyor
            _logger.LogInformation("[SendMessage] Sending to group conversation_{ConversationId}", conversationId);
            await Clients.Group($"conversation_{conversationId}").SendAsync("ReceiveMessage", message);
            _logger.LogInformation("[SendMessage] ReceiveMessage sent successfully");

            // Konusmadaki diger kullanicilara bildirim gonder (async, hata olsa bile devam et)
            _ = Task.Run(async () =>
            {
                try
                {
                    await NotifyConversationParticipantsAsync(conversationId, message);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[SendMessage] Push notification failed but message was sent");
                }
            });

            _logger.LogInformation("[SendMessage] Completed successfully for conversation {ConversationId}", conversationId);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "[SendMessage] InvalidOperationException: {Message}", ex.Message);
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
        catch (Exception ex)
        {
            // Tüm diğer exception'ları yakala - bağlantıyı koparmamak için throw etme
            _logger.LogError(ex, "[SendMessage] Unexpected error: {Message}", ex.Message);
            await Clients.Caller.SendAsync("Error", "Mesaj gönderilirken bir hata oluştu. Lütfen tekrar deneyin.");
        }
    }

    /// <summary>
    /// Mesaj duzenle
    /// </summary>
    public async Task EditMessage(int messageId, string newContent)
    {
        _logger.LogInformation("[EditMessage] Started: MessageId={MessageId}, UserId={UserId}", messageId, UserId);

        try
        {
            if (string.IsNullOrEmpty(UserId))
            {
                await Clients.Caller.SendAsync("Error", "Kullanıcı kimliği bulunamadı.");
                return;
            }

            if (string.IsNullOrWhiteSpace(newContent))
            {
                await Clients.Caller.SendAsync("Error", "Mesaj içeriği boş olamaz.");
                return;
            }

            var message = await _messagingService.EditMessageAsync(new EditMessageDto
            {
                MessageId = messageId,
                Content = newContent
            }, UserId);

            // Konusmadaki herkese guncellenmis mesaji gonder
            await Clients.Group($"conversation_{message.ConversationId}").SendAsync("MessageEdited", message);
            _logger.LogInformation("[EditMessage] Completed successfully for message {MessageId}", messageId);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "[EditMessage] InvalidOperationException: {Message}", ex.Message);
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EditMessage] Unexpected error: {Message}", ex.Message);
            await Clients.Caller.SendAsync("Error", "Mesaj düzenlenirken bir hata oluştu.");
        }
    }

    /// <summary>
    /// Mesaj sil
    /// </summary>
    public async Task DeleteMessage(int messageId)
    {
        _logger.LogInformation("[DeleteMessage] Started: MessageId={MessageId}, UserId={UserId}", messageId, UserId);

        try
        {
            if (string.IsNullOrEmpty(UserId))
            {
                await Clients.Caller.SendAsync("Error", "Kullanıcı kimliği bulunamadı.");
                return;
            }

            // Once mesaji bul ve conversationId'yi al
            var conversationId = await _messagingService.GetMessageConversationIdAsync(messageId, UserId);
            if (conversationId == null)
            {
                await Clients.Caller.SendAsync("Error", "Mesaj bulunamadı veya silme yetkiniz yok.");
                return;
            }

            await _messagingService.DeleteMessageAsync(messageId, UserId);

            // Konusmadaki herkese silme bilgisini gonder - Frontend conversationId ve messageId bekliyor
            await Clients.Group($"conversation_{conversationId}").SendAsync("MessageDeleted", conversationId, messageId);
            _logger.LogInformation("[DeleteMessage] Completed successfully for message {MessageId}", messageId);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "[DeleteMessage] InvalidOperationException: {Message}", ex.Message);
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DeleteMessage] Unexpected error: {Message}", ex.Message);
            await Clients.Caller.SendAsync("Error", "Mesaj silinirken bir hata oluştu.");
        }
    }

    /// <summary>
    /// Yaziyor gostergesini baslat
    /// </summary>
    public async Task StartTyping(int conversationId)
    {
        try
        {
            if (string.IsNullOrEmpty(UserId)) return;

            await _messagingService.UpdateTypingStatusAsync(conversationId, UserId, true);

            // Frontend: UserTyping(conversationId, userId, userName) bekliyor
            await Clients.OthersInGroup($"conversation_{conversationId}").SendAsync("UserTyping", conversationId, UserId, UserName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[StartTyping] Error for conversation {ConversationId}", conversationId);
        }
    }

    /// <summary>
    /// Yaziyor gostergesini durdur
    /// </summary>
    public async Task StopTyping(int conversationId)
    {
        try
        {
            if (string.IsNullOrEmpty(UserId)) return;

            await _messagingService.UpdateTypingStatusAsync(conversationId, UserId, false);

            // Frontend: UserStoppedTyping(conversationId, userId) bekliyor
            await Clients.OthersInGroup($"conversation_{conversationId}").SendAsync("UserStoppedTyping", conversationId, UserId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[StopTyping] Error for conversation {ConversationId}", conversationId);
        }
    }

    /// <summary>
    /// Mesajlari okundu olarak isaretlesi
    /// </summary>
    public async Task MarkAsRead(int conversationId, int? messageId = null)
    {
        try
        {
            if (string.IsNullOrEmpty(UserId))
            {
                await Clients.Caller.SendAsync("Error", "Kullanıcı kimliği bulunamadı.");
                return;
            }

            // Once okunmamis mesajlari al (mavi tik icin gonderenlere bildirim gondermek icin)
            var unreadMessages = await _messagingService.GetUnreadMessagesForUserAsync(conversationId, UserId);

            // Mesajlari okundu olarak isaretlesi
            await _messagingService.MarkAsReadAsync(new MarkAsReadDto
            {
                ConversationId = conversationId,
                MessageId = messageId
            }, UserId);

            // Her mesajin gonderenine ayri ayri MessagesRead eventi gonder (mavi tik)
            foreach (var message in unreadMessages)
            {
                if (message.SenderId != UserId)
                {
                    await Clients.User(message.SenderId).SendAsync("MessagesRead", conversationId, UserId, message.Id);
                }
            }

            if (unreadMessages.Count > 0)
            {
                _logger.LogInformation("[MarkAsRead] Sent MessagesRead event for {Count} messages in conversation {ConversationId}",
                    unreadMessages.Count, conversationId);
            }

            // Toplam okunmamis sayisini guncelle
            var unreadCount = await _messagingService.GetTotalUnreadCountAsync(UserId);
            await Clients.Caller.SendAsync("UnreadCountUpdated", unreadCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MarkAsRead] Error for conversation {ConversationId}", conversationId);
            await Clients.Caller.SendAsync("Error", "Mesajlar okundu olarak işaretlenirken bir hata oluştu.");
        }
    }

    /// <summary>
    /// Kullanici online mi?
    /// </summary>
    public static bool IsUserOnline(string userId)
    {
        return UserConnections.TryGetValue(userId, out var connections) && connections.Count > 0;
    }

    /// <summary>
    /// Online kullanicilari getir - Instance method (Frontend cagrabilsin)
    /// </summary>
    public string[] GetOnlineUsers()
    {
        return UserConnections.Keys.ToArray();
    }

    /// <summary>
    /// Online kullanicilari getir - Static method (Backend icin)
    /// </summary>
    public static IEnumerable<string> GetOnlineUserIds()
    {
        return UserConnections.Keys.ToList();
    }

    #region Private Helpers

    private async Task NotifyConversationParticipantsAsync(int conversationId, ChatMessageDto message)
    {
        // Konusmadaki diger kullanicilara push bildirim gonder
        var conversation = await _messagingService.GetConversationAsync(conversationId, UserId);
        if (conversation == null) return;

        var otherParticipants = conversation.Participants
            .Where(p => p.UserId != UserId)
            .Select(p => p.UserId)
            .ToList();

        foreach (var participantId in otherParticipants)
        {
            // Online degilse push bildirim gonder
            if (!IsUserOnline(participantId))
            {
                try
                {
                    await _pushNotificationService.SendNewMessageNotificationAsync(
                        participantId,
                        message.SenderName,
                        message.Content.Length > 50 ? message.Content.Substring(0, 47) + "..." : message.Content,
                        conversationId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Push notification gonderilemedi: {UserId}", participantId);
                }
            }
        }
    }

    #endregion
}
