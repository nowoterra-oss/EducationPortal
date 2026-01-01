using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Course;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/curriculum-progress")]
[Produces("application/json")]
[Authorize]
public class CurriculumProgressController : ControllerBase
{
    private readonly ICurriculumProgressService _progressService;
    private readonly ILogger<CurriculumProgressController> _logger;

    public CurriculumProgressController(
        ICurriculumProgressService progressService,
        ILogger<CurriculumProgressController> logger)
    {
        _progressService = progressService;
        _logger = logger;
    }

    /// <summary>
    /// Öğrencinin ders müfredatındaki ilerlemesini getir
    /// </summary>
    [HttpGet("student/{studentId}/course/{courseId}")]
    public async Task<ActionResult<ApiResponse<List<StudentCurriculumProgressDto>>>> GetStudentProgress(
        int studentId, int courseId)
    {
        var result = await _progressService.GetStudentProgressAsync(studentId, courseId);
        return Ok(result);
    }

    /// <summary>
    /// Belirli bir konu için ilerleme detayını getir
    /// </summary>
    [HttpGet("student/{studentId}/curriculum/{curriculumId}")]
    public async Task<ActionResult<ApiResponse<StudentCurriculumProgressDto>>> GetTopicProgress(
        int studentId, int curriculumId)
    {
        var result = await _progressService.GetTopicProgressAsync(studentId, curriculumId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Konu tamamlamayı onayla (öğretmen tarafından)
    /// </summary>
    [HttpPost("approve-topic")]
    [Authorize(Roles = "Admin,Ogretmen")]
    public async Task<ActionResult<ApiResponse<bool>>> ApproveTopicCompletion(
        [FromBody] ApproveTopicDto dto)
    {
        var teacherId = GetCurrentTeacherId();
        var result = await _progressService.ApproveTopicCompletionAsync(teacherId, dto.StudentId, dto.CurriculumId);
        return Ok(result);
    }

    /// <summary>
    /// Sınav kilidini aç
    /// </summary>
    [HttpPost("unlock-exam")]
    [Authorize(Roles = "Admin,Ogretmen")]
    public async Task<ActionResult<ApiResponse<bool>>> UnlockExam(
        [FromBody] UnlockExamDto dto)
    {
        var teacherId = GetCurrentTeacherId();
        var result = await _progressService.UnlockExamAsync(teacherId, dto.StudentId, dto.CurriculumId);
        return Ok(result);
    }

    /// <summary>
    /// Sınav tamamlama
    /// </summary>
    [HttpPost("complete-exam")]
    public async Task<ActionResult<ApiResponse<bool>>> CompleteExam(
        [FromBody] CompleteExamDto dto)
    {
        var result = await _progressService.CompleteExamAsync(dto.StudentId, dto.CurriculumId, dto.Score);
        return Ok(result);
    }

    /// <summary>
    /// İlerlemeyi kontrol et ve güncelle
    /// </summary>
    [HttpPost("check-update")]
    public async Task<ActionResult<ApiResponse<bool>>> CheckAndUpdateProgress(
        [FromBody] CheckProgressDto dto)
    {
        var result = await _progressService.CheckAndUpdateProgressAsync(dto.StudentId, dto.CurriculumId);
        return Ok(result);
    }

    private int GetCurrentTeacherId()
    {
        var teacherIdClaim = User.FindFirst("TeacherId")?.Value;
        return int.TryParse(teacherIdClaim, out var teacherId) ? teacherId : 0;
    }
}

// DTOs
public class ApproveTopicDto
{
    public int StudentId { get; set; }
    public int CurriculumId { get; set; }
}

public class UnlockExamDto
{
    public int StudentId { get; set; }
    public int CurriculumId { get; set; }
}

public class CompleteExamDto
{
    public int StudentId { get; set; }
    public int CurriculumId { get; set; }
    public int Score { get; set; }
}

public class CheckProgressDto
{
    public int StudentId { get; set; }
    public int CurriculumId { get; set; }
}
