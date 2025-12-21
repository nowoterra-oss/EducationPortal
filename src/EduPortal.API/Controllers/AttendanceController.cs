using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Attendance;
using EduPortal.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Attendance management endpoints
/// </summary>
[ApiController]
[Route("api/attendance")]
[Produces("application/json")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;
    private readonly ILogger<AttendanceController> _logger;

    public AttendanceController(IAttendanceService attendanceService, ILogger<AttendanceController> logger)
    {
        _attendanceService = attendanceService;
        _logger = logger;
    }

    /// <summary>
    /// Get all attendance records with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated list of attendance records</returns>
    /// <response code="200">Attendance records retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<AttendanceDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<PagedResponse<AttendanceDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _attendanceService.GetAllAsync(pageNumber, pageSize);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all attendance records");
            return StatusCode(500, ApiResponse<PagedResponse<AttendanceDto>>.ErrorResponse("Devamsızlık kayıtları getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get attendance record by ID
    /// </summary>
    /// <param name="id">Attendance record ID</param>
    /// <returns>Attendance record details</returns>
    /// <response code="200">Attendance record retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Attendance record not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<AttendanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<AttendanceDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AttendanceDto>>> GetById(int id)
    {
        try
        {
            var result = await _attendanceService.GetByIdAsync(id);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting attendance record by ID: {AttendanceId}", id);
            return StatusCode(500, ApiResponse<AttendanceDto>.ErrorResponse("Devamsızlık kaydı getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Record attendance for a student
    /// </summary>
    /// <param name="attendanceCreateDto">Attendance data</param>
    /// <returns>Created attendance record</returns>
    /// <response code="201">Attendance recorded successfully</response>
    /// <response code="400">Invalid data</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    [HttpPost]
    [Authorize(Roles = "Ogretmen,Admin")]
    [ProducesResponseType(typeof(ApiResponse<AttendanceDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<AttendanceDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<AttendanceDto>>> Create([FromBody] AttendanceCreateDto attendanceCreateDto)
    {
        try
        {
            var teacherId = GetCurrentTeacherId();
            var result = await _attendanceService.RecordAttendanceAsync(attendanceCreateDto, teacherId);

            if (result.Success)
            {
                return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while recording attendance");
            return StatusCode(500, ApiResponse<AttendanceDto>.ErrorResponse("Devamsızlık kaydedilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Bulk record attendance for multiple students
    /// </summary>
    /// <param name="attendanceList">List of attendance records</param>
    /// <returns>Created attendance records</returns>
    /// <response code="201">Attendance records created successfully</response>
    /// <response code="400">Invalid data</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    [HttpPost("bulk")]
    [Authorize(Roles = "Ogretmen,Admin")]
    [ProducesResponseType(typeof(ApiResponse<List<AttendanceDto>>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<List<AttendanceDto>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<AttendanceDto>>>> BulkCreate([FromBody] List<AttendanceCreateDto> attendanceList)
    {
        try
        {
            var teacherId = GetCurrentTeacherId();
            var result = await _attendanceService.BulkCreateAsync(attendanceList, teacherId);

            if (result.Success)
            {
                return CreatedAtAction(nameof(GetAll), new { }, result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while bulk recording attendance");
            return StatusCode(500, ApiResponse<List<AttendanceDto>>.ErrorResponse("Toplu devamsızlık kaydedilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Update attendance record
    /// </summary>
    /// <param name="id">Attendance record ID</param>
    /// <param name="attendanceCreateDto">Updated attendance data</param>
    /// <returns>Updated attendance record</returns>
    /// <response code="200">Attendance record updated successfully</response>
    /// <response code="400">Invalid data</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="404">Attendance record not found</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Ogretmen,Admin")]
    [ProducesResponseType(typeof(ApiResponse<AttendanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AttendanceDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<AttendanceDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AttendanceDto>>> Update(int id, [FromBody] AttendanceCreateDto attendanceCreateDto)
    {
        try
        {
            var result = await _attendanceService.UpdateAsync(id, attendanceCreateDto);

            if (result.Success)
            {
                return Ok(result);
            }

            return result.Message.Contains("bulunamadı") ? NotFound(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating attendance record: {AttendanceId}", id);
            return StatusCode(500, ApiResponse<AttendanceDto>.ErrorResponse("Devamsızlık kaydı güncellenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Delete attendance record
    /// </summary>
    /// <param name="id">Attendance record ID</param>
    /// <returns>Deletion confirmation</returns>
    /// <response code="200">Attendance record deleted successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="404">Attendance record not found</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _attendanceService.DeleteAsync(id);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting attendance record: {AttendanceId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Devamsızlık kaydı silinirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get attendance records for a specific student with optional date range filtering
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="startDate">Start date for filtering (optional)</param>
    /// <param name="endDate">End date for filtering (optional)</param>
    /// <returns>List of attendance records</returns>
    /// <response code="200">Attendance records retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("student/{studentId}")]
    [ProducesResponseType(typeof(ApiResponse<List<AttendanceDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<List<AttendanceDto>>>> GetByStudent(
        int studentId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var result = await _attendanceService.GetStudentAttendanceAsync(studentId, startDate, endDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting attendance records for student: {StudentId}", studentId);
            return StatusCode(500, ApiResponse<List<AttendanceDto>>.ErrorResponse("Öğrenci devamsızlık kayıtları getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get attendance records by date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="courseId">Course ID (optional)</param>
    /// <returns>List of attendance records</returns>
    /// <response code="200">Attendance records retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    [HttpGet("date-range")]
    [Authorize(Roles = "Admin,Ogretmen,Danisman")]
    [ProducesResponseType(typeof(ApiResponse<List<AttendanceDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<AttendanceDto>>>> GetByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? courseId = null)
    {
        try
        {
            var result = await _attendanceService.GetByDateRangeAsync(startDate, endDate, courseId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting attendance records by date range");
            return StatusCode(500, ApiResponse<List<AttendanceDto>>.ErrorResponse("Tarih aralığına göre devamsızlık kayıtları getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get attendance records for a specific course
    /// </summary>
    /// <param name="courseId">Course ID</param>
    /// <returns>List of attendance records</returns>
    /// <response code="200">Attendance records retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("course/{courseId}")]
    [Authorize(Roles = "Admin,Ogretmen,Danisman")]
    [ProducesResponseType(typeof(ApiResponse<List<AttendanceDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<List<AttendanceDto>>>> GetByCourse(int courseId)
    {
        try
        {
            var result = await _attendanceService.GetCourseAttendanceAsync(courseId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting attendance records for course: {CourseId}", courseId);
            return StatusCode(500, ApiResponse<List<AttendanceDto>>.ErrorResponse("Ders devamsızlık kayıtları getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get attendance summary for a student
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <returns>Attendance summary with statistics</returns>
    /// <response code="200">Attendance summary retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Student not found</response>
    [HttpGet("student/{studentId}/summary")]
    [ProducesResponseType(typeof(ApiResponse<Dictionary<string, int>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<Dictionary<string, int>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Dictionary<string, int>>>> GetStudentSummary(int studentId)
    {
        try
        {
            var result = await _attendanceService.GetAttendanceSummaryAsync(studentId);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting attendance summary for student: {StudentId}", studentId);
            return StatusCode(500, ApiResponse<Dictionary<string, int>>.ErrorResponse("Devamsızlık özeti getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Öğretmenin bugünkü yoklama özetini getirir
    /// </summary>
    /// <param name="teacherId">Öğretmen ID</param>
    /// <returns>Ders bazında gruplandırılmış yoklama özeti</returns>
    /// <response code="200">Özet başarıyla getirildi</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("today-summary/{teacherId}")]
    [Authorize(Roles = "Admin,Ogretmen")]
    [ProducesResponseType(typeof(ApiResponse<List<TodayAttendanceSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<List<TodayAttendanceSummaryDto>>>> GetTodaySummary(int teacherId)
    {
        try
        {
            var result = await _attendanceService.GetTodaySummaryAsync(teacherId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting today's attendance summary for teacher: {TeacherId}", teacherId);
            return StatusCode(500, ApiResponse<List<TodayAttendanceSummaryDto>>.ErrorResponse("Bugünkü yoklama özeti getirilirken bir hata oluştu"));
        }
    }

    private int GetCurrentTeacherId()
    {
        var teacherIdClaim = User.FindFirst("TeacherId")?.Value;
        return int.TryParse(teacherIdClaim, out var teacherId) ? teacherId : 1;
    }
}
