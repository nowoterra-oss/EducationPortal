using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Homework;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/homework-assignments")]
[Produces("application/json")]
[Authorize]
public class HomeworkAssignmentsController : ControllerBase
{
    private readonly IHomeworkAssignmentService _service;
    private readonly ILogger<HomeworkAssignmentsController> _logger;

    public HomeworkAssignmentsController(
        IHomeworkAssignmentService service,
        ILogger<HomeworkAssignmentsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Öğretmenin öğrencilerini getir (dropdown için)
    /// </summary>
    [HttpGet("teacher/{teacherId}/students")]
    [Authorize(Roles = "Admin,Ogretmen")]
    public async Task<ActionResult<ApiResponse<List<StudentSummaryDto>>>> GetTeacherStudents(int teacherId)
    {
        var result = await _service.GetTeacherStudentsAsync(teacherId);
        return Ok(result);
    }

    /// <summary>
    /// Öğretmenin ödevlerini listele
    /// </summary>
    [HttpGet("teacher/{teacherId}")]
    [Authorize(Roles = "Admin,Ogretmen")]
    public async Task<ActionResult<ApiResponse<PagedResponse<HomeworkAssignmentDto>>>> GetTeacherAssignments(
        int teacherId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetTeacherAssignmentsAsync(teacherId, pageNumber, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Öğrencinin ödevlerini listele
    /// </summary>
    [HttpGet("student/{studentId}")]
    public async Task<ActionResult<ApiResponse<PagedResponse<HomeworkAssignmentDto>>>> GetStudentAssignments(
        int studentId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetStudentAssignmentsAsync(studentId, pageNumber, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Yeni ödev ata
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Ogretmen")]
    public async Task<ActionResult<ApiResponse<HomeworkAssignmentDto>>> CreateAssignment(
        [FromBody] CreateHomeworkAssignmentDto dto)
    {
        var teacherId = GetCurrentTeacherId();
        var result = await _service.CreateAssignmentAsync(teacherId, dto);
        return result.Success
            ? CreatedAtAction(nameof(GetAssignmentDetail), new { id = result.Data?.Id }, result)
            : BadRequest(result);
    }

    /// <summary>
    /// Toplu ödev ata
    /// </summary>
    [HttpPost("bulk")]
    [Authorize(Roles = "Admin,Ogretmen")]
    public async Task<ActionResult<ApiResponse<List<HomeworkAssignmentDto>>>> CreateBulkAssignments(
        [FromBody] BulkCreateHomeworkAssignmentDto dto)
    {
        var teacherId = GetCurrentTeacherId();
        var result = await _service.CreateBulkAssignmentsAsync(teacherId, dto);
        return Ok(result);
    }

    /// <summary>
    /// Öğrencinin ödev geçmişini getir
    /// </summary>
    [HttpGet("student/{studentId}/history")]
    public async Task<ActionResult<ApiResponse<List<HomeworkAssignmentDto>>>> GetStudentHistory(
        int studentId, [FromQuery] int? teacherId)
    {
        var result = await _service.GetStudentAssignmentHistoryAsync(studentId, teacherId ?? GetCurrentTeacherId());
        return Ok(result);
    }

    /// <summary>
    /// Ödevi görüldü olarak işaretle
    /// </summary>
    [HttpPost("{id}/view")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsViewed(int id)
    {
        var studentId = GetCurrentStudentId();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers["User-Agent"].ToString();
        var result = await _service.MarkAsViewedAsync(id, studentId, ipAddress, userAgent);
        return Ok(result);
    }

    /// <summary>
    /// Kontrol bekleyen ödevler
    /// </summary>
    [HttpGet("pending-reviews")]
    [Authorize(Roles = "Admin,Ogretmen")]
    public async Task<ActionResult<ApiResponse<PagedResponse<HomeworkAssignmentDto>>>> GetPendingReviews(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var teacherId = GetCurrentTeacherId();
        var result = await _service.GetPendingReviewsAsync(teacherId, pageNumber, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Ödev detayı
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<HomeworkAssignmentDto>>> GetAssignmentDetail(int id)
    {
        var result = await _service.GetAssignmentDetailAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Ödev değerlendir
    /// </summary>
    [HttpPut("{id}/grade")]
    [Authorize(Roles = "Admin,Ogretmen")]
    public async Task<ActionResult<ApiResponse<HomeworkAssignmentDto>>> GradeAssignment(
        int id, [FromBody] GradeHomeworkDto dto)
    {
        dto.AssignmentId = id;
        var teacherId = GetCurrentTeacherId();
        var result = await _service.GradeAssignmentAsync(teacherId, dto);
        return Ok(result);
    }

    /// <summary>
    /// Öğrenci performansı
    /// </summary>
    [HttpGet("student/{studentId}/performance")]
    public async Task<ActionResult<ApiResponse<StudentHomeworkPerformanceDto>>> GetStudentPerformance(
        int studentId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var result = await _service.GetStudentPerformanceAsync(studentId, startDate, endDate);
        return Ok(result);
    }

    /// <summary>
    /// Performans chart verisi
    /// </summary>
    [HttpGet("student/{studentId}/performance/chart")]
    public async Task<ActionResult<ApiResponse<HomeworkPerformanceChartDto>>> GetPerformanceChart(
        int studentId, [FromQuery] int months = 6)
    {
        var result = await _service.GetPerformanceChartDataAsync(studentId, months);
        return Ok(result);
    }

    /// <summary>
    /// Ödev teslim et
    /// </summary>
    [HttpPost("{id}/submit")]
    public async Task<ActionResult<ApiResponse<HomeworkAssignmentDto>>> SubmitAssignment(
        int id, [FromBody] SubmitHomeworkDto dto)
    {
        var studentId = GetCurrentStudentId();
        var result = await _service.SubmitAssignmentAsync(id, studentId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Ödev için dosya yükle
    /// </summary>
    [HttpPost("{id}/upload")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB
    public async Task<ActionResult<ApiResponse<FileUploadResultDto>>> UploadFile(
        int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<FileUploadResultDto>.ErrorResponse("Dosya seçilmedi"));

        var studentId = GetCurrentStudentId();
        using var stream = file.OpenReadStream();
        var result = await _service.UploadSubmissionFileAsync(id, studentId, stream, file.FileName, file.ContentType);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Hatırlatma gönder (manuel tetikleme)
    /// </summary>
    [HttpPost("send-reminders")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<int>>> SendReminders()
    {
        var result = await _service.SendDueDateRemindersAsync();
        return Ok(result);
    }

    private int GetCurrentTeacherId()
    {
        // TODO: JWT'den al - şimdilik claims'den Teacher Id çekilecek
        var teacherIdClaim = User.FindFirst("TeacherId")?.Value;
        return int.TryParse(teacherIdClaim, out var teacherId) ? teacherId : 1;
    }

    private int GetCurrentStudentId()
    {
        // TODO: JWT'den al - şimdilik claims'den Student Id çekilecek
        var studentIdClaim = User.FindFirst("StudentId")?.Value;
        return int.TryParse(studentIdClaim, out var studentId) ? studentId : 1;
    }
}
