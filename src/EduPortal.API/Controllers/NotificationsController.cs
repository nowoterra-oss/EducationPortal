using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Notification;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduPortal.API.Controllers;

/// <summary>
/// Notification management endpoints
/// </summary>
[ApiController]
[Route("api/notifications")]
[Produces("application/json")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(INotificationService notificationService, ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    private string GetCurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("Kullanıcı kimliği bulunamadı");

    /// <summary>
    /// Get user notifications
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<NotificationDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<NotificationDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var userId = GetCurrentUserId();
            var (items, totalCount) = await _notificationService.GetAllPagedAsync(userId, pageNumber, pageSize);

            var pagedResponse = new PagedResponse<NotificationDto>(
                items.ToList(),
                totalCount,
                pageNumber,
                pageSize);

            return Ok(ApiResponse<PagedResponse<NotificationDto>>.SuccessResponse(pagedResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bildirimler getirilirken hata oluştu");
            return StatusCode(500, ApiResponse<PagedResponse<NotificationDto>>.ErrorResponse("Bildirimler getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get notification by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<NotificationDto>>> GetById(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var notification = await _notificationService.GetByIdAsync(id, userId);

            if (notification == null)
                return NotFound(ApiResponse<NotificationDto>.ErrorResponse("Bildirim bulunamadı"));

            return Ok(ApiResponse<NotificationDto>.SuccessResponse(notification));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bildirim getirilirken hata oluştu. ID: {NotificationId}", id);
            return StatusCode(500, ApiResponse<NotificationDto>.ErrorResponse("Bildirim getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Mark notification as read
    /// </summary>
    [HttpPatch("{id}/read")]
    [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<NotificationDto>>> MarkAsRead(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var notification = await _notificationService.MarkAsReadAsync(id, userId);

            if (notification == null)
                return NotFound(ApiResponse<NotificationDto>.ErrorResponse("Bildirim bulunamadı"));

            return Ok(ApiResponse<NotificationDto>.SuccessResponse(notification, "Bildirim okundu olarak işaretlendi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bildirim okundu işaretlenirken hata oluştu. ID: {NotificationId}", id);
            return StatusCode(500, ApiResponse<NotificationDto>.ErrorResponse("İşlem sırasında bir hata oluştu"));
        }
    }

    /// <summary>
    /// Mark all notifications as read
    /// </summary>
    [HttpPatch("read-all")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<int>>> MarkAllAsRead()
    {
        try
        {
            var userId = GetCurrentUserId();
            var count = await _notificationService.MarkAllAsReadAsync(userId);

            return Ok(ApiResponse<int>.SuccessResponse(count, $"{count} bildirim okundu olarak işaretlendi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tüm bildirimler okundu işaretlenirken hata oluştu");
            return StatusCode(500, ApiResponse<int>.ErrorResponse("İşlem sırasında bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get unread notifications count
    /// </summary>
    [HttpGet("unread/count")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
    {
        try
        {
            var userId = GetCurrentUserId();
            var count = await _notificationService.GetUnreadCountAsync(userId);

            return Ok(ApiResponse<int>.SuccessResponse(count));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Okunmamış bildirim sayısı getirilirken hata oluştu");
            return StatusCode(500, ApiResponse<int>.ErrorResponse("İşlem sırasında bir hata oluştu"));
        }
    }

    /// <summary>
    /// Delete notification
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _notificationService.DeleteAsync(id, userId);

            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Bildirim bulunamadı"));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Bildirim başarıyla silindi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bildirim silinirken hata oluştu. ID: {NotificationId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Bildirim silinirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Send notification to a user (Admin only)
    /// </summary>
    [HttpPost("send")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<NotificationDto>>> Send([FromBody] CreateNotificationDto notificationDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<NotificationDto>.ErrorResponse("Geçersiz veri"));

            var notification = await _notificationService.SendAsync(notificationDto);

            return CreatedAtAction(nameof(GetById), new { id = notification.Id },
                ApiResponse<NotificationDto>.SuccessResponse(notification, "Bildirim başarıyla gönderildi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bildirim gönderilirken hata oluştu");
            return StatusCode(500, ApiResponse<NotificationDto>.ErrorResponse("Bildirim gönderilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Send bulk notifications (Admin only)
    /// </summary>
    [HttpPost("bulk")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> SendBulk([FromBody] BulkNotificationDto bulkDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<int>.ErrorResponse("Geçersiz veri"));

            var sentCount = await _notificationService.SendBulkAsync(bulkDto);

            return CreatedAtAction(nameof(GetAll), null,
                ApiResponse<int>.SuccessResponse(sentCount, $"{sentCount} bildirim başarıyla gönderildi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Toplu bildirim gönderilirken hata oluştu");
            return StatusCode(500, ApiResponse<int>.ErrorResponse("Toplu bildirim gönderilirken bir hata oluştu"));
        }
    }
}
