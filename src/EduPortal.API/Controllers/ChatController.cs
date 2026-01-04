using EduPortal.Application.DTOs.Messaging;
using EduPortal.Application.Interfaces.Messaging;
using EduPortal.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduPortal.API.Controllers;

/// <summary>
/// Site ici mesajlasma API controller'i - Real-time chat
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IMessagingService _messagingService;
    private readonly IBroadcastService _broadcastService;
    private readonly IAdminMessagingService _adminMessagingService;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ILogger<ChatController> _logger;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    public ChatController(
        IMessagingService messagingService,
        IBroadcastService broadcastService,
        IAdminMessagingService adminMessagingService,
        IPushNotificationService pushNotificationService,
        ILogger<ChatController> logger)
    {
        _messagingService = messagingService;
        _broadcastService = broadcastService;
        _adminMessagingService = adminMessagingService;
        _pushNotificationService = pushNotificationService;
        _logger = logger;
    }

    #region Conversations

    /// <summary>
    /// Kullanicinin konusmalarini listeler
    /// </summary>
    [HttpGet("conversations")]
    public async Task<ActionResult<List<ConversationListDto>>> GetConversations(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var conversations = await _messagingService.GetConversationsAsync(UserId, page, pageSize);
        return Ok(conversations);
    }

    /// <summary>
    /// Konusma detayini getirir
    /// </summary>
    [HttpGet("conversations/{id}")]
    public async Task<ActionResult<ConversationDto>> GetConversation(int id)
    {
        var conversation = await _messagingService.GetConversationAsync(id, UserId);

        if (conversation == null)
        {
            return NotFound("Konuşma bulunamadı veya erişim yetkiniz yok.");
        }

        return Ok(conversation);
    }

    /// <summary>
    /// Yeni konusma olusturur
    /// </summary>
    [HttpPost("conversations")]
    public async Task<ActionResult<ConversationDto>> CreateConversation([FromBody] CreateConversationDto dto)
    {
        try
        {
            var conversation = await _messagingService.CreateConversationAsync(dto, UserId);
            return CreatedAtAction(nameof(GetConversation), new { id = conversation.Id }, conversation);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Iki kullanici arasindaki mevcut konusmayi bulur veya yeni olusturur
    /// </summary>
    [HttpPost("conversations/direct/{recipientUserId}")]
    public async Task<ActionResult<ConversationDto>> GetOrCreateDirectConversation(string recipientUserId)
    {
        try
        {
            var conversation = await _messagingService.GetOrCreateDirectConversationAsync(UserId, recipientUserId);
            return Ok(conversation);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Grup icin konusma olusturur veya mevcutu getirir
    /// </summary>
    [HttpPost("conversations/group/{groupId}")]
    public async Task<ActionResult<ConversationDto>> GetOrCreateGroupConversation(int groupId)
    {
        try
        {
            var conversation = await _messagingService.GetOrCreateGroupConversationAsync(groupId, UserId);
            return Ok(conversation);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Konusmayi siler
    /// </summary>
    [HttpDelete("conversations/{id}")]
    public async Task<ActionResult> DeleteConversation(int id)
    {
        try
        {
            await _messagingService.DeleteConversationAsync(id, UserId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Konusmayi sessize alir/acar
    /// </summary>
    [HttpPut("conversations/{id}/mute")]
    public async Task<ActionResult> MuteConversation(int id, [FromQuery] bool mute = true)
    {
        try
        {
            await _messagingService.MuteConversationAsync(id, UserId, mute);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Konusmayi sabitler/kaldirir
    /// </summary>
    [HttpPut("conversations/{id}/pin")]
    public async Task<ActionResult> PinConversation(int id, [FromQuery] bool pin = true)
    {
        try
        {
            await _messagingService.PinConversationAsync(id, UserId, pin);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    #endregion

    #region Chat Messages

    /// <summary>
    /// Konusmadaki mesajlari getirir
    /// </summary>
    [HttpGet("conversations/{conversationId}/messages")]
    public async Task<ActionResult<List<ChatMessageDto>>> GetMessages(
        int conversationId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var messages = await _messagingService.GetMessagesAsync(conversationId, UserId, page, pageSize);
            return Ok(messages);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Mesaj gonderir (REST alternatifi - SignalR tercih edilir)
    /// </summary>
    [HttpPost("conversations/{conversationId}/messages")]
    public async Task<ActionResult<ChatMessageDto>> SendMessage(int conversationId, [FromBody] SendMessageDto dto)
    {
        try
        {
            dto.ConversationId = conversationId;
            var message = await _messagingService.SendMessageAsync(dto, UserId);
            return CreatedAtAction(nameof(GetMessages), new { conversationId }, message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Mesaji duzenler
    /// </summary>
    [HttpPut("messages/{messageId}")]
    public async Task<ActionResult<ChatMessageDto>> EditMessage(int messageId, [FromBody] EditMessageDto dto)
    {
        try
        {
            dto.MessageId = messageId;
            var message = await _messagingService.EditMessageAsync(dto, UserId);
            return Ok(message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Mesaji siler
    /// </summary>
    [HttpDelete("messages/{messageId}")]
    public async Task<ActionResult> DeleteMessage(int messageId)
    {
        try
        {
            await _messagingService.DeleteMessageAsync(messageId, UserId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Mesajlari okundu olarak isaretler
    /// </summary>
    [HttpPut("conversations/{conversationId}/read")]
    public async Task<ActionResult> MarkAsRead(int conversationId, [FromQuery] int? messageId = null)
    {
        await _messagingService.MarkAsReadAsync(new MarkAsReadDto
        {
            ConversationId = conversationId,
            MessageId = messageId
        }, UserId);

        return NoContent();
    }

    #endregion

    #region Contacts

    /// <summary>
    /// Mesaj atabileceğim kisileri getirir
    /// </summary>
    [HttpGet("contacts")]
    public async Task<ActionResult<ContactListDto>> GetContacts()
    {
        var contacts = await _messagingService.GetContactsAsync(UserId);
        return Ok(contacts);
    }

    /// <summary>
    /// Kisi/grup arar
    /// </summary>
    [HttpGet("contacts/search")]
    public async Task<ActionResult<ContactListDto>> SearchContacts([FromQuery] string q)
    {
        var contacts = await _messagingService.SearchContactsAsync(UserId, q ?? "");
        return Ok(contacts);
    }

    #endregion

    #region Unread Count

    /// <summary>
    /// Toplam okunmamis mesaj sayisini getirir
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        var count = await _messagingService.GetTotalUnreadCountAsync(UserId);
        return Ok(count);
    }

    /// <summary>
    /// Konusma bazli okunmamis mesaj sayilarini getirir
    /// </summary>
    [HttpGet("unread-counts")]
    public async Task<ActionResult<Dictionary<int, int>>> GetUnreadCounts()
    {
        var counts = await _messagingService.GetUnreadCountsAsync(UserId);
        return Ok(counts);
    }

    #endregion

    #region Broadcast Messages

    /// <summary>
    /// Toplu mesaj gonderir (sadece Admin)
    /// </summary>
    [HttpPost("broadcast")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BroadcastMessageDto>> SendBroadcast([FromBody] CreateBroadcastMessageDto dto)
    {
        try
        {
            var message = await _broadcastService.SendBroadcastAsync(dto, UserId);
            return CreatedAtAction(nameof(GetBroadcast), new { id = message.Id }, message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Belirli bir kullaniciya toplu mesaj gonderir (sadece Admin)
    /// </summary>
    [HttpPost("broadcast/direct/{recipientUserId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BroadcastMessageDto>> SendDirectBroadcast(
        string recipientUserId,
        [FromBody] CreateBroadcastMessageDto dto)
    {
        try
        {
            var message = await _broadcastService.SendDirectBroadcastAsync(
                recipientUserId, dto.Title ?? "", dto.Content, UserId);
            return Ok(message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Kullanicinin aldigi toplu mesajlari listeler
    /// </summary>
    [HttpGet("broadcast")]
    public async Task<ActionResult<List<BroadcastMessageListDto>>> GetBroadcasts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var messages = await _broadcastService.GetBroadcastMessagesAsync(UserId, page, pageSize);
        return Ok(messages);
    }

    /// <summary>
    /// Toplu mesaj detayini getirir
    /// </summary>
    [HttpGet("broadcast/{id}")]
    public async Task<ActionResult<BroadcastMessageDto>> GetBroadcast(int id)
    {
        var message = await _broadcastService.GetBroadcastMessageAsync(id, UserId);

        if (message == null)
        {
            return NotFound();
        }

        return Ok(message);
    }

    /// <summary>
    /// Toplu mesaji okundu olarak isaretler
    /// </summary>
    [HttpPut("broadcast/{id}/read")]
    public async Task<ActionResult> MarkBroadcastAsRead(int id)
    {
        await _broadcastService.MarkBroadcastAsReadAsync(id, UserId);
        return NoContent();
    }

    /// <summary>
    /// Admin icin gonderilen toplu mesajlari listeler
    /// </summary>
    [HttpGet("broadcast/sent")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<BroadcastMessageListDto>>> GetSentBroadcasts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var messages = await _broadcastService.GetSentBroadcastsAsync(UserId, page, pageSize);
        return Ok(messages);
    }

    /// <summary>
    /// Okunmamis toplu mesaj sayisini getirir
    /// </summary>
    [HttpGet("broadcast/unread-count")]
    public async Task<ActionResult<int>> GetUnreadBroadcastCount()
    {
        var count = await _broadcastService.GetUnreadBroadcastCountAsync(UserId);
        return Ok(count);
    }

    #endregion

    #region Push Notifications

    /// <summary>
    /// Push notification subscription kaydeder
    /// </summary>
    [HttpPost("push/subscribe")]
    public async Task<ActionResult<PushSubscriptionDto>> Subscribe([FromBody] CreatePushSubscriptionDto dto)
    {
        var subscription = await _pushNotificationService.SubscribeAsync(dto, UserId);
        return Ok(subscription);
    }

    /// <summary>
    /// Push notification subscription siler
    /// </summary>
    [HttpDelete("push/subscribe")]
    public async Task<ActionResult> Unsubscribe([FromQuery] string endpoint)
    {
        await _pushNotificationService.UnsubscribeAsync(endpoint, UserId);
        return NoContent();
    }

    /// <summary>
    /// Kullanicinin push subscriptionlarini getirir
    /// </summary>
    [HttpGet("push/subscriptions")]
    public async Task<ActionResult<List<PushSubscriptionDto>>> GetSubscriptions()
    {
        var subscriptions = await _pushNotificationService.GetSubscriptionsAsync(UserId);
        return Ok(subscriptions);
    }

    #endregion

    #region Admin Operations

    /// <summary>
    /// Admin icin konusmayi okur ve loglar
    /// </summary>
    [HttpPost("admin/conversations/{conversationId}/read")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AdminConversationDetailDto>> AdminReadConversation(
        int conversationId,
        [FromBody] AdminReadConversationDto dto)
    {
        try
        {
            dto.ConversationId = conversationId;
            var conversation = await _adminMessagingService.ReadConversationAsync(
                dto,
                UserId,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers.UserAgent);

            return Ok(conversation);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Admin erisim loglarini listeler
    /// </summary>
    [HttpGet("admin/access-logs")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<AdminMessageAccessLogDto>>> GetAccessLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? adminUserId = null,
        [FromQuery] int? conversationId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var logs = await _adminMessagingService.GetAccessLogsAsync(
            page, pageSize, adminUserId, conversationId, startDate, endDate);
        return Ok(logs);
    }

    /// <summary>
    /// Tum konusmalari listeler (Admin icin)
    /// </summary>
    [HttpGet("admin/conversations")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<ConversationListDto>>> GetAllConversations(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? searchTerm = null)
    {
        var conversations = await _adminMessagingService.GetAllConversationsAsync(page, pageSize, searchTerm);
        return Ok(conversations);
    }

    /// <summary>
    /// Belirli bir kullanicinin konusmalarini listeler (Admin icin)
    /// </summary>
    [HttpGet("admin/users/{userId}/conversations")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<ConversationListDto>>> GetUserConversations(
        string userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var conversations = await _adminMessagingService.GetUserConversationsAsync(userId, page, pageSize);
        return Ok(conversations);
    }

    #endregion
}
