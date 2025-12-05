using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Schedule;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Ders programı yönetimi endpoint'leri (LessonSchedule)
/// </summary>
[ApiController]
[Route("api/schedules")]
[Produces("application/json")]
[Authorize]
public class SchedulesController : ControllerBase
{
    private readonly IScheduleService _scheduleService;
    private readonly ILogger<SchedulesController> _logger;

    public SchedulesController(IScheduleService scheduleService, ILogger<SchedulesController> logger)
    {
        _scheduleService = scheduleService;
        _logger = logger;
    }

    /// <summary>
    /// Tüm ders programlarını listele
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<ScheduleDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<ScheduleDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var (items, totalCount) = await _scheduleService.GetAllPagedAsync(pageNumber, pageSize);
            var response = new PagedResponse<ScheduleDto>(items.ToList(), pageNumber, pageSize, totalCount);
            return Ok(ApiResponse<PagedResponse<ScheduleDto>>.SuccessResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving schedules");
            return StatusCode(500, ApiResponse<PagedResponse<ScheduleDto>>.ErrorResponse("Programlar alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// ID ile program detayı getir
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ScheduleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ScheduleDto>>> GetById(int id)
    {
        try
        {
            var schedule = await _scheduleService.GetByIdAsync(id);
            if (schedule == null)
                return NotFound(ApiResponse<ScheduleDto>.ErrorResponse("Program bulunamadı"));

            return Ok(ApiResponse<ScheduleDto>.SuccessResponse(schedule));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving schedule {ScheduleId}", id);
            return StatusCode(500, ApiResponse<ScheduleDto>.ErrorResponse("Program alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Öğrencinin ders programını getir
    /// </summary>
    /// <param name="studentId">Öğrenci ID</param>
    /// <param name="startDate">Başlangıç tarihi (opsiyonel, format: yyyy-MM-dd)</param>
    /// <param name="endDate">Bitiş tarihi (opsiyonel, format: yyyy-MM-dd)</param>
    [HttpGet("student/{studentId}")]
    [ProducesResponseType(typeof(ApiResponse<List<ScheduleDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ScheduleDto>>>> GetByStudent(
        int studentId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var schedules = await _scheduleService.GetByStudentAsync(studentId, startDate, endDate);
            return Ok(ApiResponse<List<ScheduleDto>>.SuccessResponse(schedules.ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving schedules for student {StudentId}", studentId);
            return StatusCode(500, ApiResponse<List<ScheduleDto>>.ErrorResponse("Öğrenci programı alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Öğretmenin ders programını getir
    /// </summary>
    /// <param name="teacherId">Öğretmen ID</param>
    /// <param name="startDate">Başlangıç tarihi (opsiyonel, format: yyyy-MM-dd)</param>
    /// <param name="endDate">Bitiş tarihi (opsiyonel, format: yyyy-MM-dd)</param>
    [HttpGet("teacher/{teacherId}")]
    [ProducesResponseType(typeof(ApiResponse<List<ScheduleDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ScheduleDto>>>> GetByTeacher(
        int teacherId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var schedules = await _scheduleService.GetByTeacherAsync(teacherId, startDate, endDate);
            return Ok(ApiResponse<List<ScheduleDto>>.SuccessResponse(schedules.ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving schedules for teacher {TeacherId}", teacherId);
            return StatusCode(500, ApiResponse<List<ScheduleDto>>.ErrorResponse("Öğretmen programı alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Yeni ders programı oluştur
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<ScheduleDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ScheduleDto>>> Create([FromBody] CreateScheduleDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<ScheduleDto>.ErrorResponse("Geçersiz veri"));

            var schedule = await _scheduleService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = schedule.Id },
                ApiResponse<ScheduleDto>.SuccessResponse(schedule, "Program başarıyla oluşturuldu"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ScheduleDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<ScheduleDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating schedule");
            return StatusCode(500, ApiResponse<ScheduleDto>.ErrorResponse("Program oluşturulurken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Program bilgilerini güncelle
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<ScheduleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ScheduleDto>>> Update(int id, [FromBody] UpdateScheduleDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<ScheduleDto>.ErrorResponse("Geçersiz veri"));

            var schedule = await _scheduleService.UpdateAsync(id, dto);
            return Ok(ApiResponse<ScheduleDto>.SuccessResponse(schedule, "Program başarıyla güncellendi"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<ScheduleDto>.ErrorResponse("Program bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating schedule {ScheduleId}", id);
            return StatusCode(500, ApiResponse<ScheduleDto>.ErrorResponse("Program güncellenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Programı sil
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _scheduleService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Program bulunamadı"));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Program başarıyla silindi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting schedule {ScheduleId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Program silinirken bir hata oluştu"));
        }
    }
}
