using EduPortal.API.Attributes;
using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Homework;
using EduPortal.Application.Interfaces;
using EduPortal.Application.Services.Interfaces;
using EduPortal.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduPortal.API.Controllers;

/// <summary>
/// Homework management endpoints
/// </summary>
[ApiController]
[Route("api/homeworks")]
[Produces("application/json")]
[Authorize]
public class HomeworksController : ControllerBase
{
    private readonly IHomeworkService _homeworkService;
    private readonly ITeacherRepository _teacherRepository;
    private readonly ILogger<HomeworksController> _logger;

    public HomeworksController(
        IHomeworkService homeworkService,
        ITeacherRepository teacherRepository,
        ILogger<HomeworksController> logger)
    {
        _homeworkService = homeworkService;
        _teacherRepository = teacherRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all homeworks with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated list of homeworks</returns>
    [HttpGet]
    [RequirePermission(Permissions.AssignmentsView)]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<HomeworkDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PagedResponse<HomeworkDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _homeworkService.GetAllAsync(pageNumber, pageSize);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all homeworks");
            return StatusCode(500, ApiResponse<PagedResponse<HomeworkDto>>.ErrorResponse("�devler getirilirken bir hata olu_tu"));
        }
    }

    /// <summary>
    /// Get homework by ID
    /// </summary>
    /// <param name="id">Homework ID</param>
    /// <returns>Homework details</returns>
    [HttpGet("{id}")]
    [RequirePermission(Permissions.AssignmentsView)]
    [ProducesResponseType(typeof(ApiResponse<HomeworkDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HomeworkDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HomeworkDto>>> GetById(int id)
    {
        try
        {
            var result = await _homeworkService.GetByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting homework by ID: {HomeworkId}", id);
            return StatusCode(500, ApiResponse<HomeworkDto>.ErrorResponse("�dev getirilirken bir hata olu_tu"));
        }
    }

    /// <summary>
    /// Get all homeworks for a specific course
    /// </summary>
    /// <param name="courseId">Course ID</param>
    /// <returns>List of homeworks for the course</returns>
    [HttpGet("course/{courseId}")]
    [RequirePermission(Permissions.AssignmentsView)]
    [ProducesResponseType(typeof(ApiResponse<List<HomeworkDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<HomeworkDto>>>> GetByCourse(int courseId)
    {
        try
        {
            var result = await _homeworkService.GetByCourseAsync(courseId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting homeworks for course: {CourseId}", courseId);
            return StatusCode(500, ApiResponse<List<HomeworkDto>>.ErrorResponse("Ders �devleri getirilirken bir hata olu_tu"));
        }
    }

    /// <summary>
    /// Get all homeworks for a specific student
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <returns>List of homeworks for the student</returns>
    [HttpGet("student/{studentId}")]
    [RequirePermission(Permissions.AssignmentsView)]
    [ProducesResponseType(typeof(ApiResponse<List<HomeworkDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<HomeworkDto>>>> GetByStudent(int studentId)
    {
        try
        {
            var result = await _homeworkService.GetByStudentAsync(studentId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting homeworks for student: {StudentId}", studentId);
            return StatusCode(500, ApiResponse<List<HomeworkDto>>.ErrorResponse("�renci �devleri getirilirken bir hata olu_tu"));
        }
    }

    /// <summary>
    /// Create a new homework
    /// </summary>
    /// <param name="dto">Homework creation data</param>
    /// <returns>Created homework</returns>
    [HttpPost]
    [RequirePermission(Permissions.AssignmentsCreate)]
    [ProducesResponseType(typeof(ApiResponse<HomeworkDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<HomeworkDto>>> Create([FromBody] HomeworkCreateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<HomeworkDto>.ErrorResponse("Ge�ersiz veri"));
            }

            // Get user ID from JWT claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(ApiResponse<HomeworkDto>.ErrorResponse("Kullanıcı bilgisi bulunamadı"));
            }

            // Find teacher by user ID
            var teacher = await _teacherRepository.GetByUserIdAsync(userId);
            if (teacher == null)
            {
                return BadRequest(ApiResponse<HomeworkDto>.ErrorResponse("�retmen bilgisi bulunamad1"));
            }

            var result = await _homeworkService.CreateAsync(dto, teacher.Id);

            if (result.Success)
            {
                return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating homework");
            return StatusCode(500, ApiResponse<HomeworkDto>.ErrorResponse("�dev olu_turulurken bir hata olu_tu"));
        }
    }

    /// <summary>
    /// Update an existing homework
    /// </summary>
    /// <param name="id">Homework ID</param>
    /// <param name="dto">Homework update data</param>
    /// <returns>Updated homework</returns>
    [HttpPut("{id}")]
    [RequirePermission(Permissions.AssignmentsCreate)]
    [ProducesResponseType(typeof(ApiResponse<HomeworkDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HomeworkDto>>> Update(int id, [FromBody] HomeworkUpdateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<HomeworkDto>.ErrorResponse("Ge�ersiz veri"));
            }

            if (id != dto.Id)
            {
                return BadRequest(ApiResponse<HomeworkDto>.ErrorResponse("ID uyu_mazl11"));
            }

            var result = await _homeworkService.UpdateAsync(dto);
            return result.Success ? Ok(result) : NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating homework: {HomeworkId}", id);
            return StatusCode(500, ApiResponse<HomeworkDto>.ErrorResponse("�dev g�ncellenirken bir hata olu_tu"));
        }
    }

    /// <summary>
    /// Delete a homework
    /// </summary>
    /// <param name="id">Homework ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    [RequirePermission(Permissions.AssignmentsCreate)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _homeworkService.DeleteAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting homework: {HomeworkId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("�dev silinirken bir hata olu_tu"));
        }
    }

    /// <summary>
    /// Get all submissions for a specific homework
    /// </summary>
    /// <param name="homeworkId">Homework ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated list of submissions</returns>
    [HttpGet("{homeworkId}/submissions")]
    [RequirePermission(Permissions.AssignmentsGrade)]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<HomeworkSubmissionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<HomeworkSubmissionDto>>>> GetSubmissions(
        int homeworkId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _homeworkService.GetSubmissionsAsync(homeworkId, pageNumber, pageSize);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting submissions for homework: {HomeworkId}", homeworkId);
            return StatusCode(500, ApiResponse<PagedResponse<HomeworkSubmissionDto>>.ErrorResponse("Teslimler getirilirken bir hata olu_tu"));
        }
    }

    /// <summary>
    /// Submit homework (student)
    /// </summary>
    /// <param name="dto">Submission data</param>
    /// <returns>Submission details</returns>
    [HttpPost("submit")]
    [Authorize(Roles = "Admin,Ogrenci")]
    [ProducesResponseType(typeof(ApiResponse<HomeworkSubmissionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<HomeworkSubmissionDto>>> Submit([FromBody] HomeworkSubmitDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<HomeworkSubmissionDto>.ErrorResponse("Ge�ersiz veri"));
            }

            var result = await _homeworkService.SubmitHomeworkAsync(dto);
            return result.Success ? CreatedAtAction(nameof(GetSubmissions), new { homeworkId = dto.HomeworkId }, result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while submitting homework");
            return StatusCode(500, ApiResponse<HomeworkSubmissionDto>.ErrorResponse("�dev teslim edilirken bir hata olu_tu"));
        }
    }

    /// <summary>
    /// Grade a homework submission (teacher)
    /// </summary>
    /// <param name="dto">Grading data</param>
    /// <returns>Updated submission with grade</returns>
    [HttpPut("submissions/grade")]
    [RequirePermission(Permissions.AssignmentsGrade)]
    [ProducesResponseType(typeof(ApiResponse<HomeworkSubmissionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HomeworkSubmissionDto>>> GradeSubmission([FromBody] GradeSubmissionDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<HomeworkSubmissionDto>.ErrorResponse("Ge�ersiz veri"));
            }

            var result = await _homeworkService.GradeSubmissionAsync(dto);
            return result.Success ? Ok(result) : NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while grading submission: {SubmissionId}", dto.SubmissionId);
            return StatusCode(500, ApiResponse<HomeworkSubmissionDto>.ErrorResponse("�dev notland1r1l1rken bir hata olu_tu"));
        }
    }

    /// <summary>
    /// Get all submissions for a specific student
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <returns>List of student submissions</returns>
    [HttpGet("student/{studentId}/submissions")]
    [RequirePermission(Permissions.AssignmentsView)]
    [ProducesResponseType(typeof(ApiResponse<List<HomeworkSubmissionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<HomeworkSubmissionDto>>>> GetStudentSubmissions(int studentId)
    {
        try
        {
            var result = await _homeworkService.GetStudentSubmissionsAsync(studentId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting student submissions: {StudentId}", studentId);
            return StatusCode(500, ApiResponse<List<HomeworkSubmissionDto>>.ErrorResponse("�renci teslimleri getirilirken bir hata olu_tu"));
        }
    }
}
