using System.Security.Claims;
using EduPortal.API.Attributes;
using EduPortal.Application.Common;
using EduPortal.Application.DTOs.StudentTeacherAssignment;
using EduPortal.Application.DTOs.Teacher;
using EduPortal.Application.Interfaces;
using EduPortal.Application.Services.Interfaces;
using EduPortal.Domain.Constants;
using EduPortal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Teacher management endpoints
/// </summary>
[ApiController]
[Route("api/teachers")]
[Produces("application/json")]
[Authorize]
public class TeachersController : ControllerBase
{
    private readonly ITeacherService _teacherService;
    private readonly IStudentTeacherAssignmentService _assignmentService;
    private readonly ITeacherRepository _teacherRepository;
    private readonly ILogger<TeachersController> _logger;

    public TeachersController(
        ITeacherService teacherService,
        IStudentTeacherAssignmentService assignmentService,
        ITeacherRepository teacherRepository,
        ILogger<TeachersController> logger)
    {
        _teacherService = teacherService;
        _assignmentService = assignmentService;
        _teacherRepository = teacherRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all teachers with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="includeInactive">Include inactive teachers (default: false)</param>
    /// <returns>Paginated list of teachers</returns>
    /// <response code="200">Teachers retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    [HttpGet]
    [RequirePermission(Permissions.TeachersView, Permissions.UsersView, Permissions.SchedulingView, Permissions.SchedulingCreate)]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<TeacherDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<PagedResponse<TeacherDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool includeInactive = false)
    {
        try
        {
            var result = await _teacherService.GetAllAsync(pageNumber, pageSize, includeInactive);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all teachers");
            return StatusCode(500, ApiResponse<PagedResponse<TeacherDto>>.ErrorResponse("Öğretmenler getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get teacher by ID
    /// </summary>
    /// <param name="id">Teacher ID</param>
    /// <returns>Teacher details</returns>
    /// <response code="200">Teacher retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Teacher not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<TeacherDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<TeacherDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TeacherDto>>> GetById(int id)
    {
        try
        {
            var result = await _teacherService.GetByIdAsync(id);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting teacher by ID: {TeacherId}", id);
            return StatusCode(500, ApiResponse<TeacherDto>.ErrorResponse("Öğretmen bilgisi getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Search teachers by name, email, or specialization
    /// </summary>
    /// <param name="term">Search term</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="includeInactive">Include inactive teachers (default: false)</param>
    /// <returns>Matching teachers</returns>
    /// <response code="200">Search completed successfully</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("search")]
    [RequirePermission(Permissions.TeachersView, Permissions.UsersView, Permissions.SchedulingView, Permissions.SchedulingCreate)]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<TeacherDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PagedResponse<TeacherDto>>>> Search(
        [FromQuery] string term,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool includeInactive = false)
    {
        try
        {
            var result = await _teacherService.SearchAsync(term, includeInactive);
            var pagedResponse = new PagedResponse<TeacherDto>(
                result.Data ?? new List<TeacherDto>(),
                result.Data?.Count ?? 0,
                pageNumber,
                pageSize
            );
            return Ok(ApiResponse<PagedResponse<TeacherDto>>.SuccessResponse(pagedResponse, "Arama tamamlandı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching teachers with term: {SearchTerm}", term);
            return StatusCode(500, ApiResponse<PagedResponse<TeacherDto>>.ErrorResponse("Öğretmen arama sırasında bir hata oluştu"));
        }
    }

    /// <summary>
    /// Create a new teacher
    /// </summary>
    /// <param name="teacherCreateDto">Teacher creation data</param>
    /// <returns>Created teacher</returns>
    /// <response code="201">Teacher created successfully</response>
    /// <response code="400">Invalid data</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    [HttpPost]
    [RequirePermission(Permissions.TeachersCreate, Permissions.UsersCreate)]
    [ProducesResponseType(typeof(ApiResponse<TeacherDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<TeacherDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<TeacherDto>>> Create([FromBody] TeacherCreateDto teacherCreateDto)
    {
        try
        {
            var result = await _teacherService.CreateAsync(teacherCreateDto);

            if (result.Success)
            {
                return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating teacher");
            return StatusCode(500, ApiResponse<TeacherDto>.ErrorResponse("Öğretmen oluşturulurken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Update teacher information
    /// </summary>
    /// <param name="id">Teacher ID</param>
    /// <param name="teacherUpdateDto">Updated teacher data</param>
    /// <returns>Updated teacher</returns>
    /// <response code="200">Teacher updated successfully</response>
    /// <response code="400">Invalid data</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="404">Teacher not found</response>
    [HttpPut("{id}")]
    [RequirePermission(Permissions.TeachersEdit, Permissions.UsersEdit)]
    [ProducesResponseType(typeof(ApiResponse<TeacherDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TeacherDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<TeacherDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TeacherDto>>> Update(int id, [FromBody] TeacherUpdateDto teacherUpdateDto)
    {
        try
        {
            teacherUpdateDto.Id = id;
            var result = await _teacherService.UpdateAsync(teacherUpdateDto);

            if (result.Success)
            {
                return Ok(result);
            }

            return result.Message.Contains("bulunamadı") ? NotFound(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating teacher: {TeacherId}", id);
            return StatusCode(500, ApiResponse<TeacherDto>.ErrorResponse("Öğretmen güncellenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Delete teacher
    /// </summary>
    /// <param name="id">Teacher ID</param>
    /// <returns>Deletion confirmation</returns>
    /// <response code="200">Teacher deleted successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="404">Teacher not found</response>
    [HttpDelete("{id}")]
    [RequirePermission(Permissions.TeachersDelete, Permissions.UsersDelete)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _teacherService.DeleteAsync(id);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting teacher: {TeacherId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Öğretmen silinirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get teacher's courses
    /// </summary>
    /// <param name="id">Teacher ID</param>
    /// <returns>List of teacher's courses</returns>
    /// <response code="200">Courses retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Teacher not found</response>
    [HttpGet("{id}/courses")]
    [ProducesResponseType(typeof(ApiResponse<List<object>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<List<object>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetCourses(int id)
    {
        try
        {
            var result = await _teacherService.GetTeacherCoursesAsync(id);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting courses for teacher: {TeacherId}", id);
            return StatusCode(500, ApiResponse<List<object>>.ErrorResponse("Öğretmen dersleri getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get teacher's assigned students
    /// </summary>
    /// <param name="id">Teacher ID</param>
    /// <returns>List of students assigned to this teacher</returns>
    /// <response code="200">Assigned students retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Teacher not found</response>
    [HttpGet("{id}/assigned-students")]
    [ProducesResponseType(typeof(ApiResponse<List<StudentTeacherAssignmentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<List<StudentTeacherAssignmentDto>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<List<StudentTeacherAssignmentDto>>>> GetAssignedStudents(int id)
    {
        try
        {
            // Önce öğretmenin var olup olmadığını kontrol et
            var teacher = await _teacherService.GetByIdAsync(id);
            if (!teacher.Success)
            {
                return NotFound(ApiResponse<List<StudentTeacherAssignmentDto>>.ErrorResponse("Öğretmen bulunamadı"));
            }

            var assignments = await _assignmentService.GetByTeacherIdAsync(id);
            var result = assignments.ToList();

            return Ok(ApiResponse<List<StudentTeacherAssignmentDto>>.SuccessResponse(result, "Atanmış öğrenciler başarıyla getirildi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting assigned students for teacher: {TeacherId}", id);
            return StatusCode(500, ApiResponse<List<StudentTeacherAssignmentDto>>.ErrorResponse("Atanmış öğrenciler getirilirken bir hata oluştu"));
        }
    }

    // ===============================================
    // CURRENT TEACHER (ME) ENDPOINTS
    // ===============================================

    /// <summary>
    /// Giriş yapmış öğretmenin bilgilerini döndürür
    /// </summary>
    /// <returns>Öğretmen bilgileri</returns>
    /// <response code="200">Öğretmen bilgileri başarıyla getirildi</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Öğretmen bulunamadı</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<TeacherDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<TeacherDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TeacherDto>>> GetCurrentTeacher()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<TeacherDto>.ErrorResponse("Kullanıcı kimliği alınamadı"));
            }

            var teacher = await _teacherRepository.GetByUserIdAsync(userId);
            if (teacher == null)
            {
                return NotFound(ApiResponse<TeacherDto>.ErrorResponse("Öğretmen profili bulunamadı"));
            }

            var result = await _teacherService.GetByIdAsync(teacher.Id);
            return result.Success ? Ok(result) : NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting current teacher");
            return StatusCode(500, ApiResponse<TeacherDto>.ErrorResponse("Öğretmen bilgisi getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Giriş yapmış öğretmenin danışmanlık yaptığı öğrencileri döndürür
    /// </summary>
    /// <returns>Danışmanlık yapılan öğrenciler listesi</returns>
    /// <response code="200">Danışman öğrencileri başarıyla getirildi</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Öğretmen bulunamadı</response>
    [HttpGet("me/advisor-students")]
    [ProducesResponseType(typeof(ApiResponse<List<StudentTeacherAssignmentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<List<StudentTeacherAssignmentDto>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<List<StudentTeacherAssignmentDto>>>> GetAdvisorStudents()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<List<StudentTeacherAssignmentDto>>.ErrorResponse("Kullanıcı kimliği alınamadı"));
            }

            var teacher = await _teacherRepository.GetByUserIdAsync(userId);
            if (teacher == null)
            {
                return NotFound(ApiResponse<List<StudentTeacherAssignmentDto>>.ErrorResponse("Öğretmen profili bulunamadı"));
            }

            var assignments = await _assignmentService.GetByTeacherIdAsync(teacher.Id);
            var advisorStudents = assignments
                .Where(a => a.AssignmentType == AssignmentType.Advisor && a.IsActive)
                .ToList();

            return Ok(ApiResponse<List<StudentTeacherAssignmentDto>>.SuccessResponse(advisorStudents, "Danışman öğrencileri başarıyla getirildi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting advisor students");
            return StatusCode(500, ApiResponse<List<StudentTeacherAssignmentDto>>.ErrorResponse("Danışman öğrencileri getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Giriş yapmış öğretmenin rehberlik yaptığı öğrencileri döndürür
    /// </summary>
    /// <returns>Rehberlik yapılan öğrenciler listesi</returns>
    /// <response code="200">Rehber öğrencileri başarıyla getirildi</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Öğretmen bulunamadı</response>
    [HttpGet("me/counselor-students")]
    [ProducesResponseType(typeof(ApiResponse<List<StudentTeacherAssignmentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<List<StudentTeacherAssignmentDto>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<List<StudentTeacherAssignmentDto>>>> GetCounselorStudents()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<List<StudentTeacherAssignmentDto>>.ErrorResponse("Kullanıcı kimliği alınamadı"));
            }

            var teacher = await _teacherRepository.GetByUserIdAsync(userId);
            if (teacher == null)
            {
                return NotFound(ApiResponse<List<StudentTeacherAssignmentDto>>.ErrorResponse("Öğretmen profili bulunamadı"));
            }

            var assignments = await _assignmentService.GetByTeacherIdAsync(teacher.Id);
            var counselorStudents = assignments
                .Where(a => a.AssignmentType == AssignmentType.Counselor && a.IsActive)
                .ToList();

            return Ok(ApiResponse<List<StudentTeacherAssignmentDto>>.SuccessResponse(counselorStudents, "Rehber öğrencileri başarıyla getirildi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting counselor students");
            return StatusCode(500, ApiResponse<List<StudentTeacherAssignmentDto>>.ErrorResponse("Rehber öğrencileri getirilirken bir hata oluştu"));
        }
    }
}
