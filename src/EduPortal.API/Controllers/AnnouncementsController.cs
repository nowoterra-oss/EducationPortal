using EduPortal.API.Attributes;
using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Announcement;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Constants;
using EduPortal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduPortal.API.Controllers;

/// <summary>
/// Duyuru yönetimi endpoint'leri
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class AnnouncementsController : ControllerBase
{
    private readonly IAnnouncementService _announcementService;
    private readonly ILogger<AnnouncementsController> _logger;

    public AnnouncementsController(IAnnouncementService announcementService, ILogger<AnnouncementsController> logger)
    {
        _announcementService = announcementService;
        _logger = logger;
    }

    /// <summary>
    /// Tüm duyuruları listele
    /// </summary>
    [HttpGet]
    [RequirePermission(Permissions.AnnouncementsView)]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<AnnouncementDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<AnnouncementDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] AnnouncementType? type = null)
    {
        try
        {
            var (items, totalCount) = await _announcementService.GetAllPagedAsync(pageNumber, pageSize, type);
            var response = new PagedResponse<AnnouncementDto>(items.ToList(), pageNumber, pageSize, totalCount);
            return Ok(ApiResponse<PagedResponse<AnnouncementDto>>.SuccessResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving announcements");
            return StatusCode(500, ApiResponse<PagedResponse<AnnouncementDto>>.ErrorResponse("Duyurular alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// ID ile duyuru detayı getir
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission(Permissions.AnnouncementsView)]
    [ProducesResponseType(typeof(ApiResponse<AnnouncementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AnnouncementDto>>> GetById(int id)
    {
        try
        {
            var announcement = await _announcementService.GetByIdAsync(id);
            if (announcement == null)
                return NotFound(ApiResponse<AnnouncementDto>.ErrorResponse("Duyuru bulunamadı"));

            return Ok(ApiResponse<AnnouncementDto>.SuccessResponse(announcement));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving announcement {AnnouncementId}", id);
            return StatusCode(500, ApiResponse<AnnouncementDto>.ErrorResponse("Duyuru alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Aktif duyuruları listele
    /// </summary>
    [HttpGet("active")]
    [RequirePermission(Permissions.AnnouncementsView)]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<AnnouncementDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<AnnouncementDto>>>> GetActive(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var (items, totalCount) = await _announcementService.GetActiveAsync(pageNumber, pageSize);
            var response = new PagedResponse<AnnouncementDto>(items.ToList(), pageNumber, pageSize, totalCount);
            return Ok(ApiResponse<PagedResponse<AnnouncementDto>>.SuccessResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active announcements");
            return StatusCode(500, ApiResponse<PagedResponse<AnnouncementDto>>.ErrorResponse("Aktif duyurular alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Sabitlenmiş duyuruları listele
    /// </summary>
    [HttpGet("pinned")]
    [RequirePermission(Permissions.AnnouncementsView)]
    [ProducesResponseType(typeof(ApiResponse<List<AnnouncementDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<AnnouncementDto>>>> GetPinned()
    {
        try
        {
            var announcements = await _announcementService.GetPinnedAsync();
            return Ok(ApiResponse<List<AnnouncementDto>>.SuccessResponse(announcements.ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pinned announcements");
            return StatusCode(500, ApiResponse<List<AnnouncementDto>>.ErrorResponse("Sabitlenmiş duyurular alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Yeni duyuru oluştur (Admin only)
    /// </summary>
    [HttpPost]
    [RequirePermission(Permissions.AnnouncementsCreate)]
    [ProducesResponseType(typeof(ApiResponse<AnnouncementDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AnnouncementDto>>> Create([FromBody] CreateAnnouncementDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<AnnouncementDto>.ErrorResponse("Geçersiz veri"));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var announcement = await _announcementService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = announcement.Id },
                ApiResponse<AnnouncementDto>.SuccessResponse(announcement, "Duyuru başarıyla oluşturuldu"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating announcement");
            return StatusCode(500, ApiResponse<AnnouncementDto>.ErrorResponse("Duyuru oluşturulurken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Duyuru bilgilerini güncelle (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [RequirePermission(Permissions.AnnouncementsEdit)]
    [ProducesResponseType(typeof(ApiResponse<AnnouncementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AnnouncementDto>>> Update(int id, [FromBody] UpdateAnnouncementDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<AnnouncementDto>.ErrorResponse("Geçersiz veri"));

            var announcement = await _announcementService.UpdateAsync(id, dto);
            return Ok(ApiResponse<AnnouncementDto>.SuccessResponse(announcement, "Duyuru başarıyla güncellendi"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<AnnouncementDto>.ErrorResponse("Duyuru bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating announcement {AnnouncementId}", id);
            return StatusCode(500, ApiResponse<AnnouncementDto>.ErrorResponse("Duyuru güncellenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Duyuruyu sabitle (Admin only)
    /// </summary>
    [HttpPatch("{id}/pin")]
    [RequirePermission(Permissions.AnnouncementsEdit)]
    [ProducesResponseType(typeof(ApiResponse<AnnouncementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AnnouncementDto>>> Pin(int id)
    {
        try
        {
            var announcement = await _announcementService.PinAsync(id);
            return Ok(ApiResponse<AnnouncementDto>.SuccessResponse(announcement, "Duyuru sabitlendi"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<AnnouncementDto>.ErrorResponse("Duyuru bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pinning announcement {AnnouncementId}", id);
            return StatusCode(500, ApiResponse<AnnouncementDto>.ErrorResponse("Duyuru sabitlenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Duyurunun sabitlemeyi kaldır (Admin only)
    /// </summary>
    [HttpPatch("{id}/unpin")]
    [RequirePermission(Permissions.AnnouncementsEdit)]
    [ProducesResponseType(typeof(ApiResponse<AnnouncementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AnnouncementDto>>> Unpin(int id)
    {
        try
        {
            var announcement = await _announcementService.UnpinAsync(id);
            return Ok(ApiResponse<AnnouncementDto>.SuccessResponse(announcement, "Duyuru sabitlemesi kaldırıldı"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<AnnouncementDto>.ErrorResponse("Duyuru bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unpinning announcement {AnnouncementId}", id);
            return StatusCode(500, ApiResponse<AnnouncementDto>.ErrorResponse("Duyuru sabitlemesi kaldırılırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Duyuruyu sil (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [RequirePermission(Permissions.AnnouncementsEdit)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _announcementService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Duyuru bulunamadı"));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Duyuru başarıyla silindi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting announcement {AnnouncementId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Duyuru silinirken bir hata oluştu"));
        }
    }
}
