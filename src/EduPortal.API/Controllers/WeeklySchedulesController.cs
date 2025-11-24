using EduPortal.Application.Common;
using EduPortal.Application.DTOs.WeeklySchedule;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Haftalık ders programı yönetimi endpoint'leri
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class WeeklySchedulesController : ControllerBase
{
    private readonly IWeeklyScheduleService _weeklyScheduleService;
    private readonly ILogger<WeeklySchedulesController> _logger;

    public WeeklySchedulesController(IWeeklyScheduleService weeklyScheduleService, ILogger<WeeklySchedulesController> logger)
    {
        _weeklyScheduleService = weeklyScheduleService;
        _logger = logger;
    }

    /// <summary>
    /// Tüm haftalık programları listele
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Öğretmen,Kayitci")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<WeeklyScheduleDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<WeeklyScheduleDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var (items, totalCount) = await _weeklyScheduleService.GetAllPagedAsync(pageNumber, pageSize);

            var pagedResponse = new PagedResponse<WeeklyScheduleDto>(
                items.ToList(),
                totalCount,
                pageNumber,
                pageSize);

            return Ok(ApiResponse<PagedResponse<WeeklyScheduleDto>>.SuccessResponse(pagedResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ders programları getirilirken hata oluştu");
            return StatusCode(500, ApiResponse<PagedResponse<WeeklyScheduleDto>>.ErrorResponse("Ders programları getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// ID ile program detayı getir
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<WeeklyScheduleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<WeeklyScheduleDto>>> GetById(int id)
    {
        try
        {
            var schedule = await _weeklyScheduleService.GetByIdAsync(id);

            if (schedule == null)
                return NotFound(ApiResponse<WeeklyScheduleDto>.ErrorResponse("Ders programı bulunamadı"));

            return Ok(ApiResponse<WeeklyScheduleDto>.SuccessResponse(schedule));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ders programı getirilirken hata oluştu. ID: {ScheduleId}", id);
            return StatusCode(500, ApiResponse<WeeklyScheduleDto>.ErrorResponse("Ders programı getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Sınıfın haftalık programını getir
    /// </summary>
    [HttpGet("class/{classId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<WeeklyScheduleDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<WeeklyScheduleDto>>>> GetByClass(int classId)
    {
        try
        {
            var schedules = await _weeklyScheduleService.GetByClassAsync(classId);
            return Ok(ApiResponse<IEnumerable<WeeklyScheduleDto>>.SuccessResponse(schedules));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sınıf programı getirilirken hata oluştu. ClassId: {ClassId}", classId);
            return StatusCode(500, ApiResponse<IEnumerable<WeeklyScheduleDto>>.ErrorResponse("Sınıf programı getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Öğretmenin haftalık programını getir
    /// </summary>
    [HttpGet("teacher/{teacherId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<WeeklyScheduleDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<WeeklyScheduleDto>>>> GetByTeacher(int teacherId)
    {
        try
        {
            var schedules = await _weeklyScheduleService.GetByTeacherAsync(teacherId);
            return Ok(ApiResponse<IEnumerable<WeeklyScheduleDto>>.SuccessResponse(schedules));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Öğretmen programı getirilirken hata oluştu. TeacherId: {TeacherId}", teacherId);
            return StatusCode(500, ApiResponse<IEnumerable<WeeklyScheduleDto>>.ErrorResponse("Öğretmen programı getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Dersliğin kullanım programını getir
    /// </summary>
    [HttpGet("classroom/{classroomId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<WeeklyScheduleDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<WeeklyScheduleDto>>>> GetByClassroom(int classroomId)
    {
        try
        {
            var schedules = await _weeklyScheduleService.GetByClassroomAsync(classroomId);
            return Ok(ApiResponse<IEnumerable<WeeklyScheduleDto>>.SuccessResponse(schedules));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Derslik programı getirilirken hata oluştu. ClassroomId: {ClassroomId}", classroomId);
            return StatusCode(500, ApiResponse<IEnumerable<WeeklyScheduleDto>>.ErrorResponse("Derslik programı getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Bugünün derslerini getir
    /// </summary>
    [HttpGet("today")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<WeeklyScheduleDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<WeeklyScheduleDto>>>> GetToday()
    {
        try
        {
            var schedules = await _weeklyScheduleService.GetTodayAsync();
            return Ok(ApiResponse<IEnumerable<WeeklyScheduleDto>>.SuccessResponse(schedules));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bugünün dersleri getirilirken hata oluştu");
            return StatusCode(500, ApiResponse<IEnumerable<WeeklyScheduleDto>>.ErrorResponse("Bugünün dersleri getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Yeni ders ekle
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Kayitci")]
    [ProducesResponseType(typeof(ApiResponse<WeeklyScheduleDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<WeeklyScheduleDto>>> Create([FromBody] CreateWeeklyScheduleDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<WeeklyScheduleDto>.ErrorResponse("Geçersiz veri"));

            var schedule = await _weeklyScheduleService.CreateAsync(createDto);

            return CreatedAtAction(nameof(GetById), new { id = schedule.Id },
                ApiResponse<WeeklyScheduleDto>.SuccessResponse(schedule, "Ders programı başarıyla oluşturuldu"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ders programı oluşturulurken hata oluştu");
            return StatusCode(500, ApiResponse<WeeklyScheduleDto>.ErrorResponse("Ders programı oluşturulurken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Toplu ders ekle
    /// </summary>
    [HttpPost("bulk")]
    [Authorize(Roles = "Admin,Kayitci")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<WeeklyScheduleDto>>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<IEnumerable<WeeklyScheduleDto>>>> CreateBulk([FromBody] List<CreateWeeklyScheduleDto> createDtos)
    {
        try
        {
            if (!ModelState.IsValid || !createDtos.Any())
                return BadRequest(ApiResponse<IEnumerable<WeeklyScheduleDto>>.ErrorResponse("Geçersiz veri"));

            var schedules = await _weeklyScheduleService.CreateBulkAsync(createDtos);

            return CreatedAtAction(nameof(GetAll), null,
                ApiResponse<IEnumerable<WeeklyScheduleDto>>.SuccessResponse(schedules, $"{schedules.Count()} ders programı başarıyla oluşturuldu"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Toplu ders programı oluşturulurken hata oluştu");
            return StatusCode(500, ApiResponse<IEnumerable<WeeklyScheduleDto>>.ErrorResponse("Toplu ders programı oluşturulurken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Ders programını güncelle
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Kayitci")]
    [ProducesResponseType(typeof(ApiResponse<WeeklyScheduleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<WeeklyScheduleDto>>> Update(int id, [FromBody] UpdateWeeklyScheduleDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<WeeklyScheduleDto>.ErrorResponse("Geçersiz veri"));

            var schedule = await _weeklyScheduleService.UpdateAsync(id, updateDto);

            return Ok(ApiResponse<WeeklyScheduleDto>.SuccessResponse(schedule, "Ders programı başarıyla güncellendi"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<WeeklyScheduleDto>.ErrorResponse("Ders programı bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ders programı güncellenirken hata oluştu. ID: {ScheduleId}", id);
            return StatusCode(500, ApiResponse<WeeklyScheduleDto>.ErrorResponse("Ders programı güncellenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Ders programını sil
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _weeklyScheduleService.DeleteAsync(id);

            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Ders programı bulunamadı"));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Ders programı başarıyla silindi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ders programı silinirken hata oluştu. ID: {ScheduleId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Ders programı silinirken bir hata oluştu"));
        }
    }
}
