using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Exam;
using EduPortal.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Internal exam management endpoints
/// </summary>
[ApiController]
[Route("api/exams/internal")]
[Produces("application/json")]
[Authorize]
public class InternalExamsController : ControllerBase
{
    private readonly IInternalExamService _examService;
    private readonly ILogger<InternalExamsController> _logger;

    public InternalExamsController(IInternalExamService examService, ILogger<InternalExamsController> logger)
    {
        _examService = examService;
        _logger = logger;
    }

    /// <summary>
    /// Get all internal exams with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated list of internal exams</returns>
    [HttpGet]
    [Authorize(Roles = "Admin,Ogretmen,Ogrenci")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<InternalExamDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PagedResponse<InternalExamDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _examService.GetAllAsync(pageNumber, pageSize);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all internal exams");
            return StatusCode(500, ApiResponse<PagedResponse<InternalExamDto>>.ErrorResponse("Sınavlar getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get internal exam by ID
    /// </summary>
    /// <param name="id">Exam ID</param>
    /// <returns>Exam details</returns>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Ogretmen,Ogrenci")]
    [ProducesResponseType(typeof(ApiResponse<InternalExamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InternalExamDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<InternalExamDto>>> GetById(int id)
    {
        try
        {
            var result = await _examService.GetByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting exam by ID: {ExamId}", id);
            return StatusCode(500, ApiResponse<InternalExamDto>.ErrorResponse("Sınav getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get all exams for a specific course
    /// </summary>
    /// <param name="courseId">Course ID</param>
    /// <returns>List of exams for the course</returns>
    [HttpGet("course/{courseId}")]
    [Authorize(Roles = "Admin,Ogretmen,Ogrenci")]
    [ProducesResponseType(typeof(ApiResponse<List<InternalExamDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<InternalExamDto>>>> GetByCourse(int courseId)
    {
        try
        {
            var result = await _examService.GetByCourseAsync(courseId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting exams for course: {CourseId}", courseId);
            return StatusCode(500, ApiResponse<List<InternalExamDto>>.ErrorResponse("Ders sınavları getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get all exams for a specific student
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <returns>List of exams for the student</returns>
    [HttpGet("student/{studentId}")]
    [Authorize(Roles = "Admin,Ogretmen,Ogrenci")]
    [ProducesResponseType(typeof(ApiResponse<List<InternalExamDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<InternalExamDto>>>> GetByStudent(int studentId)
    {
        try
        {
            var result = await _examService.GetByStudentAsync(studentId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting exams for student: {StudentId}", studentId);
            return StatusCode(500, ApiResponse<List<InternalExamDto>>.ErrorResponse("Öğrenci sınavları getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Create a new internal exam
    /// </summary>
    /// <param name="dto">Exam creation data</param>
    /// <returns>Created exam</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Ogretmen")]
    [ProducesResponseType(typeof(ApiResponse<InternalExamDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<InternalExamDto>>> Create([FromBody] InternalExamCreateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<InternalExamDto>.ErrorResponse("Geçersiz veri"));
            }

            // Get teacher ID from authenticated user claims
            var teacherIdClaim = User.FindFirst("teacherId");
            if (teacherIdClaim == null || !int.TryParse(teacherIdClaim.Value, out int teacherId))
            {
                return BadRequest(ApiResponse<InternalExamDto>.ErrorResponse("Öğretmen bilgisi bulunamadı"));
            }

            var result = await _examService.CreateAsync(dto, teacherId);

            if (result.Success)
            {
                return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating internal exam");
            return StatusCode(500, ApiResponse<InternalExamDto>.ErrorResponse("Sınav oluşturulurken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Update an existing internal exam
    /// </summary>
    /// <param name="id">Exam ID</param>
    /// <param name="dto">Exam update data</param>
    /// <returns>Updated exam</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Ogretmen")]
    [ProducesResponseType(typeof(ApiResponse<InternalExamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<InternalExamDto>>> Update(int id, [FromBody] InternalExamUpdateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<InternalExamDto>.ErrorResponse("Geçersiz veri"));
            }

            if (id != dto.Id)
            {
                return BadRequest(ApiResponse<InternalExamDto>.ErrorResponse("ID uyuşmazlığı"));
            }

            var result = await _examService.UpdateAsync(dto);
            return result.Success ? Ok(result) : NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating internal exam: {ExamId}", id);
            return StatusCode(500, ApiResponse<InternalExamDto>.ErrorResponse("Sınav güncellenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Delete an internal exam
    /// </summary>
    /// <param name="id">Exam ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Ogretmen")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _examService.DeleteAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting internal exam: {ExamId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Sınav silinirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get all results for a specific exam
    /// </summary>
    /// <param name="examId">Exam ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated list of exam results</returns>
    [HttpGet("{examId}/results")]
    [Authorize(Roles = "Admin,Ogretmen")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<ExamResultDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<ExamResultDto>>>> GetResults(
        int examId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _examService.GetResultsAsync(examId, pageNumber, pageSize);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting results for exam: {ExamId}", examId);
            return StatusCode(500, ApiResponse<PagedResponse<ExamResultDto>>.ErrorResponse("Sonuçlar getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Add a result for an exam
    /// </summary>
    /// <param name="dto">Exam result data</param>
    /// <returns>Created exam result</returns>
    [HttpPost("results")]
    [Authorize(Roles = "Admin,Ogretmen")]
    [ProducesResponseType(typeof(ApiResponse<ExamResultDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ExamResultDto>>> AddResult([FromBody] ExamResultCreateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ExamResultDto>.ErrorResponse("Geçersiz veri"));
            }

            var result = await _examService.AddResultAsync(dto);
            return result.Success ? CreatedAtAction(nameof(GetResults), new { examId = dto.ExamId }, result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding exam result");
            return StatusCode(500, ApiResponse<ExamResultDto>.ErrorResponse("Sınav sonucu eklenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get statistics for an exam
    /// </summary>
    /// <param name="examId">Exam ID</param>
    /// <returns>Exam statistics</returns>
    [HttpGet("{examId}/statistics")]
    [Authorize(Roles = "Admin,Ogretmen")]
    [ProducesResponseType(typeof(ApiResponse<ExamStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ExamStatisticsDto>>> GetStatistics(int examId)
    {
        try
        {
            var result = await _examService.GetStatisticsAsync(examId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting statistics for exam: {ExamId}", examId);
            return StatusCode(500, ApiResponse<ExamStatisticsDto>.ErrorResponse("İstatistikler getirilirken bir hata oluştu"));
        }
    }
}
