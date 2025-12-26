using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Counselor;
using EduPortal.Application.DTOs.Teacher;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Counselor management endpoints
/// </summary>
[ApiController]
[Route("api/counselors")]
[Produces("application/json")]
[Authorize]
public class CounselorsController : ControllerBase
{
    private readonly ICounselorService _counselorService;
    private readonly ILogger<CounselorsController> _logger;

    public CounselorsController(ICounselorService counselorService, ILogger<CounselorsController> logger)
    {
        _counselorService = counselorService;
        _logger = logger;
    }

    /// <summary>
    /// Get all counselors with pagination
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<CounselorSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<CounselorSummaryDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var (items, totalCount) = await _counselorService.GetCounselorsPagedAsync(pageNumber, pageSize);
            var pagedResponse = new PagedResponse<CounselorSummaryDto>(
                items.ToList(),
                totalCount,
                pageNumber,
                pageSize);

            return Ok(ApiResponse<PagedResponse<CounselorSummaryDto>>.SuccessResponse(pagedResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting counselors");
            return StatusCode(500, ApiResponse<PagedResponse<CounselorSummaryDto>>.ErrorResponse("Danışmanlar alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get active counselors
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CounselorDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CounselorDto>>>> GetActive()
    {
        try
        {
            var counselors = await _counselorService.GetActiveCounselorsAsync();
            return Ok(ApiResponse<IEnumerable<CounselorDto>>.SuccessResponse(counselors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active counselors");
            return StatusCode(500, ApiResponse<IEnumerable<CounselorDto>>.ErrorResponse("Aktif danışmanlar alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get counselor by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CounselorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CounselorDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CounselorDto>>> GetById(int id)
    {
        try
        {
            var counselor = await _counselorService.GetCounselorByIdAsync(id);
            if (counselor == null)
            {
                return NotFound(ApiResponse<CounselorDto>.ErrorResponse("Danışman bulunamadı"));
            }

            return Ok(ApiResponse<CounselorDto>.SuccessResponse(counselor));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting counselor {CounselorId}", id);
            return StatusCode(500, ApiResponse<CounselorDto>.ErrorResponse("Danışman alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Create new counselor
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<CounselorDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CounselorDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CounselorDto>>> Create([FromBody] CreateCounselorDto counselorDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<CounselorDto>.ErrorResponse("Geçersiz veri"));
            }

            var counselor = await _counselorService.CreateCounselorAsync(counselorDto);
            return CreatedAtAction(nameof(GetById), new { id = counselor.Id },
                ApiResponse<CounselorDto>.SuccessResponse(counselor, "Danışman başarıyla oluşturuldu"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating counselor");
            return StatusCode(500, ApiResponse<CounselorDto>.ErrorResponse("Danışman oluşturulurken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Create counselor from teacher
    /// </summary>
    [HttpPost("from-teacher/{teacherId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<CounselorDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CounselorDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CounselorDto>>> CreateFromTeacher(int teacherId, [FromBody] CreateCounselorFromTeacherDto dto)
    {
        try
        {
            var counselor = await _counselorService.CreateCounselorFromTeacherAsync(teacherId, dto.Specialization);
            return CreatedAtAction(nameof(GetById), new { id = counselor.Id },
                ApiResponse<CounselorDto>.SuccessResponse(counselor, "Danışman başarıyla oluşturuldu"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating counselor from teacher {TeacherId}", teacherId);
            return BadRequest(ApiResponse<CounselorDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Get teachers available for counseling
    /// </summary>
    [HttpGet("available-teachers")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TeacherSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<TeacherSummaryDto>>>> GetAvailableTeachers()
    {
        try
        {
            var teachers = await _counselorService.GetTeachersAvailableForCounselingAsync();
            return Ok(ApiResponse<IEnumerable<TeacherSummaryDto>>.SuccessResponse(teachers));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available teachers for counseling");
            return StatusCode(500, ApiResponse<IEnumerable<TeacherSummaryDto>>.ErrorResponse("Öğretmenler alınırken hata oluştu"));
        }
    }

    /// <summary>
    /// Update counselor
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<CounselorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CounselorDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CounselorDto>>> Update(int id, [FromBody] UpdateCounselorDto counselorDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<CounselorDto>.ErrorResponse("Geçersiz veri"));
            }

            var counselor = await _counselorService.UpdateCounselorAsync(id, counselorDto);
            return Ok(ApiResponse<CounselorDto>.SuccessResponse(counselor, "Danışman başarıyla güncellendi"));
        }
        catch (Exception ex) when (ex.Message == "Counselor not found")
        {
            return NotFound(ApiResponse<CounselorDto>.ErrorResponse("Danışman bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating counselor {CounselorId}", id);
            return StatusCode(500, ApiResponse<CounselorDto>.ErrorResponse("Danışman güncellenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Delete counselor
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _counselorService.DeleteCounselorAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Danışman bulunamadı"));
            }

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Danışman başarıyla silindi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting counselor {CounselorId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Danışman silinirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get counselor's assigned students
    /// </summary>
    [HttpGet("{id}/students")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CounselorStudentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CounselorStudentDto>>>> GetStudents(int id)
    {
        try
        {
            var students = await _counselorService.GetCounselorStudentsAsync(id);
            return Ok(ApiResponse<IEnumerable<CounselorStudentDto>>.SuccessResponse(students));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting students for counselor {CounselorId}", id);
            return StatusCode(500, ApiResponse<IEnumerable<CounselorStudentDto>>.ErrorResponse("Öğrenciler alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Assign student to counselor
    /// </summary>
    [HttpPost("{id}/students/{studentId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> AssignStudent(int id, int studentId)
    {
        try
        {
            var result = await _counselorService.AssignStudentAsync(id, studentId);
            if (!result)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse("Öğrenci atanamadı. Danışman bulunamadı veya öğrenci zaten atanmış."));
            }

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Öğrenci başarıyla atandı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning student {StudentId} to counselor {CounselorId}", studentId, id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Öğrenci atanırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Batch update counselor students - replaces all current assignments
    /// </summary>
    [HttpPut("{id}/students")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateStudents(int id, [FromBody] List<int> studentIds)
    {
        try
        {
            var result = await _counselorService.UpdateCounselorStudentsAsync(id, studentIds);
            if (!result)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse("Öğrenciler güncellenemedi"));
            }

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Öğrenciler başarıyla güncellendi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating students for counselor {CounselorId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Öğrenciler güncellenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Unassign student from counselor
    /// </summary>
    [HttpDelete("{id}/students/{studentId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> UnassignStudent(int id, int studentId)
    {
        try
        {
            var result = await _counselorService.UnassignStudentAsync(id, studentId);
            if (!result)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Atama bulunamadı"));
            }

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Öğrenci ataması başarıyla kaldırıldı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unassigning student {StudentId} from counselor {CounselorId}", studentId, id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Atama kaldırılırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get counselor's sessions
    /// </summary>
    [HttpGet("{id}/sessions")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<CounselingSessionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<CounselingSessionDto>>>> GetSessions(
        int id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var (items, totalCount) = await _counselorService.GetCounselorSessionsPagedAsync(id, pageNumber, pageSize);
            var pagedResponse = new PagedResponse<CounselingSessionDto>(
                items.ToList(),
                totalCount,
                pageNumber,
                pageSize);

            return Ok(ApiResponse<PagedResponse<CounselingSessionDto>>.SuccessResponse(pagedResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sessions for counselor {CounselorId}", id);
            return StatusCode(500, ApiResponse<PagedResponse<CounselingSessionDto>>.ErrorResponse("Oturumlar alınırken bir hata oluştu"));
        }
    }
}
