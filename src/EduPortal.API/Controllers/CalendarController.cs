using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Calendar;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Calendar and event management endpoints
/// </summary>
[ApiController]
[Route("api/calendar")]
[Produces("application/json")]
[Authorize]
public class CalendarController : ControllerBase
{
    private readonly ICalendarService _calendarService;
    private readonly ILogger<CalendarController> _logger;

    public CalendarController(ICalendarService calendarService, ILogger<CalendarController> logger)
    {
        _calendarService = calendarService;
        _logger = logger;
    }

    /// <summary>
    /// Get all events with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<CalendarEventDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<CalendarEventDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var (items, totalCount) = await _calendarService.GetAllPagedAsync(pageNumber, pageSize);

            var pagedResponse = new PagedResponse<CalendarEventDto>(
                items.ToList(),
                totalCount,
                pageNumber,
                pageSize);

            return Ok(ApiResponse<PagedResponse<CalendarEventDto>>.SuccessResponse(pagedResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Etkinlikler getirilirken hata oluştu");
            return StatusCode(500, ApiResponse<PagedResponse<CalendarEventDto>>.ErrorResponse("Etkinlikler getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get event by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CalendarEventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CalendarEventDto>>> GetById(int id)
    {
        try
        {
            var calendarEvent = await _calendarService.GetByIdAsync(id);

            if (calendarEvent == null)
                return NotFound(ApiResponse<CalendarEventDto>.ErrorResponse("Etkinlik bulunamadı"));

            return Ok(ApiResponse<CalendarEventDto>.SuccessResponse(calendarEvent));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Etkinlik getirilirken hata oluştu. ID: {EventId}", id);
            return StatusCode(500, ApiResponse<CalendarEventDto>.ErrorResponse("Etkinlik getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Create new event
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Öğretmen,Danışman")]
    [ProducesResponseType(typeof(ApiResponse<CalendarEventDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CalendarEventDto>>> Create([FromBody] CreateCalendarEventDto eventDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<CalendarEventDto>.ErrorResponse("Geçersiz veri"));

            var calendarEvent = await _calendarService.CreateAsync(eventDto);

            return CreatedAtAction(nameof(GetById), new { id = calendarEvent.Id },
                ApiResponse<CalendarEventDto>.SuccessResponse(calendarEvent, "Etkinlik başarıyla oluşturuldu"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Etkinlik oluşturulurken hata oluştu");
            return StatusCode(500, ApiResponse<CalendarEventDto>.ErrorResponse("Etkinlik oluşturulurken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Update event
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Öğretmen,Danışman")]
    [ProducesResponseType(typeof(ApiResponse<CalendarEventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CalendarEventDto>>> Update(int id, [FromBody] UpdateCalendarEventDto eventDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<CalendarEventDto>.ErrorResponse("Geçersiz veri"));

            var calendarEvent = await _calendarService.UpdateAsync(id, eventDto);

            return Ok(ApiResponse<CalendarEventDto>.SuccessResponse(calendarEvent, "Etkinlik başarıyla güncellendi"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<CalendarEventDto>.ErrorResponse("Etkinlik bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Etkinlik güncellenirken hata oluştu. ID: {EventId}", id);
            return StatusCode(500, ApiResponse<CalendarEventDto>.ErrorResponse("Etkinlik güncellenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Delete event
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _calendarService.DeleteAsync(id);

            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Etkinlik bulunamadı"));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Etkinlik başarıyla silindi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Etkinlik silinirken hata oluştu. ID: {EventId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Etkinlik silinirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get events by date range
    /// </summary>
    [HttpGet("range")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CalendarEventDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CalendarEventDto>>>> GetByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var events = await _calendarService.GetByDateRangeAsync(startDate, endDate);
            return Ok(ApiResponse<IEnumerable<CalendarEventDto>>.SuccessResponse(events));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tarih aralığına göre etkinlikler getirilirken hata oluştu");
            return StatusCode(500, ApiResponse<IEnumerable<CalendarEventDto>>.ErrorResponse("Etkinlikler getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get student events
    /// </summary>
    [HttpGet("student/{studentId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CalendarEventDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CalendarEventDto>>>> GetByStudent(int studentId)
    {
        try
        {
            var events = await _calendarService.GetByStudentAsync(studentId);
            return Ok(ApiResponse<IEnumerable<CalendarEventDto>>.SuccessResponse(events));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Öğrenci etkinlikleri getirilirken hata oluştu. StudentId: {StudentId}", studentId);
            return StatusCode(500, ApiResponse<IEnumerable<CalendarEventDto>>.ErrorResponse("Etkinlikler getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get upcoming events
    /// </summary>
    [HttpGet("upcoming")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CalendarEventDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CalendarEventDto>>>> GetUpcoming([FromQuery] int days = 7)
    {
        try
        {
            var events = await _calendarService.GetUpcomingAsync(days);
            return Ok(ApiResponse<IEnumerable<CalendarEventDto>>.SuccessResponse(events));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yaklaşan etkinlikler getirilirken hata oluştu");
            return StatusCode(500, ApiResponse<IEnumerable<CalendarEventDto>>.ErrorResponse("Etkinlikler getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get class events
    /// </summary>
    [HttpGet("class/{classId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CalendarEventDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CalendarEventDto>>>> GetByClass(int classId)
    {
        try
        {
            var events = await _calendarService.GetByClassAsync(classId);
            return Ok(ApiResponse<IEnumerable<CalendarEventDto>>.SuccessResponse(events));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sınıf etkinlikleri getirilirken hata oluştu. ClassId: {ClassId}", classId);
            return StatusCode(500, ApiResponse<IEnumerable<CalendarEventDto>>.ErrorResponse("Etkinlikler getirilirken bir hata oluştu"));
        }
    }
}
