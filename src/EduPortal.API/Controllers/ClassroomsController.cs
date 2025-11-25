using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Classroom;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Derslik/oda yönetimi endpoint'leri
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ClassroomsController : ControllerBase
{
    private readonly IClassroomService _classroomService;
    private readonly ILogger<ClassroomsController> _logger;

    public ClassroomsController(IClassroomService classroomService, ILogger<ClassroomsController> logger)
    {
        _classroomService = classroomService;
        _logger = logger;
    }

    /// <summary>
    /// Tüm derslikleri listele
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Öğretmen,Kayitci")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<ClassroomDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<ClassroomDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? buildingName = null,
        [FromQuery] bool? isLab = null)
    {
        try
        {
            var (items, totalCount) = await _classroomService.GetAllPagedAsync(pageNumber, pageSize, buildingName, isLab);
            var response = new PagedResponse<ClassroomDto>(items.ToList(), pageNumber, pageSize, totalCount);
            return Ok(ApiResponse<PagedResponse<ClassroomDto>>.SuccessResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving classrooms");
            return StatusCode(500, ApiResponse<PagedResponse<ClassroomDto>>.ErrorResponse("Derslikler alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// ID ile derslik detayı getir
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ClassroomDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ClassroomDto>>> GetById(int id)
    {
        try
        {
            var classroom = await _classroomService.GetByIdAsync(id);
            if (classroom == null)
                return NotFound(ApiResponse<ClassroomDto>.ErrorResponse("Derslik bulunamadı"));

            return Ok(ApiResponse<ClassroomDto>.SuccessResponse(classroom));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving classroom {ClassroomId}", id);
            return StatusCode(500, ApiResponse<ClassroomDto>.ErrorResponse("Derslik alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Müsait derslikleri getir
    /// </summary>
    [HttpGet("available")]
    [Authorize(Roles = "Admin,Öğretmen,Kayitci")]
    [ProducesResponseType(typeof(ApiResponse<List<ClassroomDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ClassroomDto>>>> GetAvailable(
        [FromQuery] DayOfWeek dayOfWeek,
        [FromQuery] TimeSpan startTime,
        [FromQuery] TimeSpan endTime)
    {
        try
        {
            var classrooms = await _classroomService.GetAvailableAsync(dayOfWeek, startTime, endTime);
            return Ok(ApiResponse<List<ClassroomDto>>.SuccessResponse(classrooms.ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available classrooms");
            return StatusCode(500, ApiResponse<List<ClassroomDto>>.ErrorResponse("Müsait derslikler alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Derslik kullanım programını getir
    /// </summary>
    [HttpGet("{id}/schedule")]
    [ProducesResponseType(typeof(ApiResponse<List<ClassroomScheduleDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ClassroomScheduleDto>>>> GetSchedule(int id)
    {
        try
        {
            var schedule = await _classroomService.GetScheduleAsync(id);
            return Ok(ApiResponse<List<ClassroomScheduleDto>>.SuccessResponse(schedule.ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving classroom schedule {ClassroomId}", id);
            return StatusCode(500, ApiResponse<List<ClassroomScheduleDto>>.ErrorResponse("Derslik programı alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Yeni derslik ekle
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Kayitci")]
    [ProducesResponseType(typeof(ApiResponse<ClassroomDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ClassroomDto>>> Create([FromBody] CreateClassroomDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<ClassroomDto>.ErrorResponse("Geçersiz veri"));

            var classroom = await _classroomService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = classroom.Id },
                ApiResponse<ClassroomDto>.SuccessResponse(classroom, "Derslik başarıyla oluşturuldu"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating classroom");
            return StatusCode(500, ApiResponse<ClassroomDto>.ErrorResponse("Derslik oluşturulurken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Derslik bilgilerini güncelle
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Kayitci")]
    [ProducesResponseType(typeof(ApiResponse<ClassroomDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ClassroomDto>>> Update(int id, [FromBody] UpdateClassroomDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<ClassroomDto>.ErrorResponse("Geçersiz veri"));

            var classroom = await _classroomService.UpdateAsync(id, dto);
            return Ok(ApiResponse<ClassroomDto>.SuccessResponse(classroom, "Derslik başarıyla güncellendi"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<ClassroomDto>.ErrorResponse("Derslik bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating classroom {ClassroomId}", id);
            return StatusCode(500, ApiResponse<ClassroomDto>.ErrorResponse("Derslik güncellenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Dersliği sil
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _classroomService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Derslik bulunamadı"));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Derslik başarıyla silindi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting classroom {ClassroomId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Derslik silinirken bir hata oluştu"));
        }
    }
}
