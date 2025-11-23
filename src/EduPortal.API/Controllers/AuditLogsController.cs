using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Audit;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Audit log yönetimi endpoint'leri
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditService _auditService;
    private readonly ILogger<AuditLogsController> _logger;

    public AuditLogsController(
        IAuditService auditService,
        ILogger<AuditLogsController> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Audit logları getir (filtreli ve sayfalı)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<AuditLogDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<AuditLogDto>>>> GetLogs(
        [FromQuery] AuditLogFilterDto filter)
    {
        try
        {
            var result = await _auditService.GetLogsAsync(filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs");
            return StatusCode(500, ApiResponse<PagedResponse<AuditLogDto>>.ErrorResponse("Sunucu hatası"));
        }
    }

    /// <summary>
    /// Audit log ID'ye göre getir
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuditLogDto>>> GetLogById(long id)
    {
        try
        {
            var result = await _auditService.GetLogByIdAsync(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting audit log {id}");
            return StatusCode(500, ApiResponse<AuditLogDto>.ErrorResponse("Sunucu hatası"));
        }
    }

    /// <summary>
    /// Kullanıcının audit loglarını getir
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(ApiResponse<List<AuditLogDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<AuditLogDto>>>> GetUserLogs(
        string userId,
        [FromQuery] int count = 50)
    {
        try
        {
            var result = await _auditService.GetUserLogsAsync(userId, count);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting user logs for {userId}");
            return StatusCode(500, ApiResponse<List<AuditLogDto>>.ErrorResponse("Sunucu hatası"));
        }
    }

    /// <summary>
    /// Entity'nin audit loglarını getir
    /// </summary>
    [HttpGet("entity/{entityType}/{entityId}")]
    [ProducesResponseType(typeof(ApiResponse<List<AuditLogDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<AuditLogDto>>>> GetEntityLogs(
        string entityType,
        string entityId,
        [FromQuery] int count = 50)
    {
        try
        {
            var result = await _auditService.GetEntityLogsAsync(entityType, entityId, count);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting entity logs for {entityType}/{entityId}");
            return StatusCode(500, ApiResponse<List<AuditLogDto>>.ErrorResponse("Sunucu hatası"));
        }
    }

    /// <summary>
    /// Audit istatistiklerini getir
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponse<AuditStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuditStatisticsDto>>> GetStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var result = await _auditService.GetStatisticsAsync(startDate, endDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit statistics");
            return StatusCode(500, ApiResponse<AuditStatisticsDto>.ErrorResponse("Sunucu hatası"));
        }
    }

    /// <summary>
    /// Son başarısız logları getir
    /// </summary>
    [HttpGet("failed/recent")]
    [ProducesResponseType(typeof(ApiResponse<List<AuditLogDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<AuditLogDto>>>> GetRecentFailedLogs(
        [FromQuery] int count = 20)
    {
        try
        {
            var result = await _auditService.GetRecentFailedLogsAsync(count);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent failed logs");
            return StatusCode(500, ApiResponse<List<AuditLogDto>>.ErrorResponse("Sunucu hatası"));
        }
    }

    /// <summary>
    /// Eski logları temizle
    /// </summary>
    [HttpDelete("cleanup")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<int>>> CleanupOldLogs(
        [FromQuery] int daysToKeep = 90)
    {
        try
        {
            var result = await _auditService.CleanupOldLogsAsync(daysToKeep);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old logs");
            return StatusCode(500, ApiResponse<int>.ErrorResponse("Sunucu hatası"));
        }
    }

    /// <summary>
    /// Manuel audit log oluştur
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<long>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<long>>> CreateLog([FromBody] CreateAuditLogDto dto)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            var result = await _auditService.LogAsync(dto, userId, userName, userEmail);

            if (result.Success)
                return CreatedAtAction(nameof(GetLogById), new { id = result.Data }, result);

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit log");
            return StatusCode(500, ApiResponse<long>.ErrorResponse("Sunucu hatası"));
        }
    }
}
