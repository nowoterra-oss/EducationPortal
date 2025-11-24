using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Message;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduPortal.API.Controllers;

/// <summary>
/// Messaging system endpoints
/// </summary>
[ApiController]
[Route("api/messages")]
[Produces("application/json")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(IMessageService messageService, ILogger<MessagesController> logger)
    {
        _messageService = messageService;
        _logger = logger;
    }

    private string GetCurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("Kullanıcı kimliği bulunamadı");

    /// <summary>
    /// Get all messages with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<MessageSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<MessageSummaryDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var userId = GetCurrentUserId();
            var (items, totalCount) = await _messageService.GetAllPagedAsync(userId, pageNumber, pageSize);

            var pagedResponse = new PagedResponse<MessageSummaryDto>(
                items.ToList(),
                totalCount,
                pageNumber,
                pageSize);

            return Ok(ApiResponse<PagedResponse<MessageSummaryDto>>.SuccessResponse(pagedResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mesajlar getirilirken hata oluştu");
            return StatusCode(500, ApiResponse<PagedResponse<MessageSummaryDto>>.ErrorResponse("Mesajlar getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get message by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<MessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<MessageDto>>> GetById(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var message = await _messageService.GetByIdAsync(id, userId);

            if (message == null)
                return NotFound(ApiResponse<MessageDto>.ErrorResponse("Mesaj bulunamadı"));

            return Ok(ApiResponse<MessageDto>.SuccessResponse(message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mesaj getirilirken hata oluştu. ID: {MessageId}", id);
            return StatusCode(500, ApiResponse<MessageDto>.ErrorResponse("Mesaj getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Send message
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<MessageDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<MessageDto>>> Send([FromBody] SendMessageDto messageDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<MessageDto>.ErrorResponse("Geçersiz veri"));

            var senderId = GetCurrentUserId();
            var message = await _messageService.SendAsync(senderId, messageDto);

            return CreatedAtAction(nameof(GetById), new { id = message.Id },
                ApiResponse<MessageDto>.SuccessResponse(message, "Mesaj başarıyla gönderildi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mesaj gönderilirken hata oluştu");
            return StatusCode(500, ApiResponse<MessageDto>.ErrorResponse("Mesaj gönderilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Reply to message
    /// </summary>
    [HttpPost("{id}/reply")]
    [ProducesResponseType(typeof(ApiResponse<MessageDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<MessageDto>>> Reply(int id, [FromBody] SendMessageDto messageDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<MessageDto>.ErrorResponse("Geçersiz veri"));

            var senderId = GetCurrentUserId();
            var message = await _messageService.ReplyAsync(id, senderId, messageDto);

            return CreatedAtAction(nameof(GetById), new { id = message.Id },
                ApiResponse<MessageDto>.SuccessResponse(message, "Yanıt başarıyla gönderildi"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<MessageDto>.ErrorResponse("Yanıtlanacak mesaj bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mesaj yanıtlanırken hata oluştu. ParentId: {ParentId}", id);
            return StatusCode(500, ApiResponse<MessageDto>.ErrorResponse("Mesaj yanıtlanırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Delete message
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _messageService.DeleteAsync(id, userId);

            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Mesaj bulunamadı"));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Mesaj başarıyla silindi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mesaj silinirken hata oluştu. ID: {MessageId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Mesaj silinirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get inbox messages
    /// </summary>
    [HttpGet("inbox")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<MessageSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<MessageSummaryDto>>>> GetInbox(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var userId = GetCurrentUserId();
            var (items, totalCount) = await _messageService.GetInboxPagedAsync(userId, pageNumber, pageSize);

            var pagedResponse = new PagedResponse<MessageSummaryDto>(
                items.ToList(),
                totalCount,
                pageNumber,
                pageSize);

            return Ok(ApiResponse<PagedResponse<MessageSummaryDto>>.SuccessResponse(pagedResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gelen kutusu getirilirken hata oluştu");
            return StatusCode(500, ApiResponse<PagedResponse<MessageSummaryDto>>.ErrorResponse("Gelen kutusu getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get sent messages
    /// </summary>
    [HttpGet("sent")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<MessageSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<MessageSummaryDto>>>> GetSent(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var userId = GetCurrentUserId();
            var (items, totalCount) = await _messageService.GetSentPagedAsync(userId, pageNumber, pageSize);

            var pagedResponse = new PagedResponse<MessageSummaryDto>(
                items.ToList(),
                totalCount,
                pageNumber,
                pageSize);

            return Ok(ApiResponse<PagedResponse<MessageSummaryDto>>.SuccessResponse(pagedResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gönderilen mesajlar getirilirken hata oluştu");
            return StatusCode(500, ApiResponse<PagedResponse<MessageSummaryDto>>.ErrorResponse("Gönderilen mesajlar getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get unread messages
    /// </summary>
    [HttpGet("unread")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<MessageSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<MessageSummaryDto>>>> GetUnread(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var userId = GetCurrentUserId();
            var (items, totalCount) = await _messageService.GetUnreadPagedAsync(userId, pageNumber, pageSize);

            var pagedResponse = new PagedResponse<MessageSummaryDto>(
                items.ToList(),
                totalCount,
                pageNumber,
                pageSize);

            return Ok(ApiResponse<PagedResponse<MessageSummaryDto>>.SuccessResponse(pagedResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Okunmamış mesajlar getirilirken hata oluştu");
            return StatusCode(500, ApiResponse<PagedResponse<MessageSummaryDto>>.ErrorResponse("Okunmamış mesajlar getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Mark message as read
    /// </summary>
    [HttpPatch("{id}/read")]
    [ProducesResponseType(typeof(ApiResponse<MessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<MessageDto>>> MarkAsRead(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var message = await _messageService.MarkAsReadAsync(id, userId);

            if (message == null)
                return NotFound(ApiResponse<MessageDto>.ErrorResponse("Mesaj bulunamadı"));

            return Ok(ApiResponse<MessageDto>.SuccessResponse(message, "Mesaj okundu olarak işaretlendi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mesaj okundu işaretlenirken hata oluştu. ID: {MessageId}", id);
            return StatusCode(500, ApiResponse<MessageDto>.ErrorResponse("İşlem sırasında bir hata oluştu"));
        }
    }

    /// <summary>
    /// Mark message as unread
    /// </summary>
    [HttpPatch("{id}/unread")]
    [ProducesResponseType(typeof(ApiResponse<MessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<MessageDto>>> MarkAsUnread(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var message = await _messageService.MarkAsUnreadAsync(id, userId);

            if (message == null)
                return NotFound(ApiResponse<MessageDto>.ErrorResponse("Mesaj bulunamadı"));

            return Ok(ApiResponse<MessageDto>.SuccessResponse(message, "Mesaj okunmadı olarak işaretlendi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mesaj okunmadı işaretlenirken hata oluştu. ID: {MessageId}", id);
            return StatusCode(500, ApiResponse<MessageDto>.ErrorResponse("İşlem sırasında bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get conversation with a user
    /// </summary>
    [HttpGet("conversation/{userId}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<MessageDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<MessageDto>>>> GetConversation(
        string userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var (items, totalCount) = await _messageService.GetConversationPagedAsync(currentUserId, userId, pageNumber, pageSize);

            var pagedResponse = new PagedResponse<MessageDto>(
                items.ToList(),
                totalCount,
                pageNumber,
                pageSize);

            return Ok(ApiResponse<PagedResponse<MessageDto>>.SuccessResponse(pagedResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Konuşma getirilirken hata oluştu. OtherUserId: {OtherUserId}", userId);
            return StatusCode(500, ApiResponse<PagedResponse<MessageDto>>.ErrorResponse("Konuşma getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Search messages
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<MessageSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<MessageSummaryDto>>>> Search(
        [FromQuery] string term,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(term))
                return BadRequest(ApiResponse<PagedResponse<MessageSummaryDto>>.ErrorResponse("Arama terimi boş olamaz"));

            var userId = GetCurrentUserId();
            var (items, totalCount) = await _messageService.SearchAsync(userId, term, pageNumber, pageSize);

            var pagedResponse = new PagedResponse<MessageSummaryDto>(
                items.ToList(),
                totalCount,
                pageNumber,
                pageSize);

            return Ok(ApiResponse<PagedResponse<MessageSummaryDto>>.SuccessResponse(pagedResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mesaj araması yapılırken hata oluştu. Term: {Term}", term);
            return StatusCode(500, ApiResponse<PagedResponse<MessageSummaryDto>>.ErrorResponse("Arama yapılırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get unread count
    /// </summary>
    [HttpGet("unread/count")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
    {
        try
        {
            var userId = GetCurrentUserId();
            var count = await _messageService.GetUnreadCountAsync(userId);

            return Ok(ApiResponse<int>.SuccessResponse(count));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Okunmamış mesaj sayısı getirilirken hata oluştu");
            return StatusCode(500, ApiResponse<int>.ErrorResponse("İşlem sırasında bir hata oluştu"));
        }
    }

    /// <summary>
    /// Send bulk message
    /// </summary>
    [HttpPost("bulk")]
    [Authorize(Roles = "Admin,Öğretmen,Danışman")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> SendBulk([FromBody] BulkMessageDto bulkMessageDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<int>.ErrorResponse("Geçersiz veri"));

            var senderId = GetCurrentUserId();
            var sentCount = await _messageService.SendBulkAsync(senderId, bulkMessageDto);

            return CreatedAtAction(nameof(GetSent), null,
                ApiResponse<int>.SuccessResponse(sentCount, $"{sentCount} mesaj başarıyla gönderildi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Toplu mesaj gönderilirken hata oluştu");
            return StatusCode(500, ApiResponse<int>.ErrorResponse("Toplu mesaj gönderilirken bir hata oluştu"));
        }
    }
}
