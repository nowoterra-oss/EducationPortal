using System.Security.Claims;
using EduPortal.Application.Common;
using EduPortal.Application.DTOs.CounselorDashboard;
using EduPortal.Application.Interfaces;
using EduPortal.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.API.Controllers;

/// <summary>
/// Danisma dashboard endpoint'leri
/// </summary>
[ApiController]
[Route("api/counselor-dashboard")]
[Produces("application/json")]
[Authorize(Roles = "Admin,Danışman")]
public class CounselorDashboardController : ControllerBase
{
    private readonly ICounselorDashboardService _dashboardService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CounselorDashboardController> _logger;

    public CounselorDashboardController(
        ICounselorDashboardService dashboardService,
        ApplicationDbContext context,
        ILogger<CounselorDashboardController> logger)
    {
        _dashboardService = dashboardService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Ogrenci tam profil bilgilerini getir
    /// </summary>
    [HttpGet("student/{studentId}/profile")]
    [ProducesResponseType(typeof(ApiResponse<StudentFullProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StudentFullProfileDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StudentFullProfileDto>>> GetStudentProfile(int studentId)
    {
        try
        {
            var result = await _dashboardService.GetStudentFullProfileAsync(studentId);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ogrenci profili getirilirken hata: {StudentId}", studentId);
            return StatusCode(500, ApiResponse<StudentFullProfileDto>.ErrorResponse("Bir hata olustu"));
        }
    }

    /// <summary>
    /// Ogrenci akademik performans bilgilerini getir
    /// </summary>
    [HttpGet("student/{studentId}/academic-performance")]
    [ProducesResponseType(typeof(ApiResponse<StudentAcademicPerformanceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<StudentAcademicPerformanceDto>>> GetAcademicPerformance(int studentId)
    {
        try
        {
            var result = await _dashboardService.GetStudentAcademicPerformanceAsync(studentId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Akademik performans getirilirken hata: {StudentId}", studentId);
            return StatusCode(500, ApiResponse<StudentAcademicPerformanceDto>.ErrorResponse("Bir hata olustu"));
        }
    }

    /// <summary>
    /// Ogrenci yurtdisi egitim bilgilerini getir
    /// </summary>
    [HttpGet("student/{studentId}/international-education")]
    [ProducesResponseType(typeof(ApiResponse<StudentInternationalEducationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<StudentInternationalEducationDto>>> GetInternationalEducation(int studentId)
    {
        try
        {
            var result = await _dashboardService.GetStudentInternationalEducationAsync(studentId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yurtdisi egitim bilgileri getirilirken hata: {StudentId}", studentId);
            return StatusCode(500, ApiResponse<StudentInternationalEducationDto>.ErrorResponse("Bir hata olustu"));
        }
    }

    /// <summary>
    /// Ogrenci aktiviteler ve oduller bilgilerini getir
    /// </summary>
    [HttpGet("student/{studentId}/activities-awards")]
    [ProducesResponseType(typeof(ApiResponse<StudentActivitiesAndAwardsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<StudentActivitiesAndAwardsDto>>> GetActivitiesAndAwards(int studentId)
    {
        try
        {
            var result = await _dashboardService.GetStudentActivitiesAndAwardsAsync(studentId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Aktiviteler ve oduller getirilirken hata: {StudentId}", studentId);
            return StatusCode(500, ApiResponse<StudentActivitiesAndAwardsDto>.ErrorResponse("Bir hata olustu"));
        }
    }

    /// <summary>
    /// Ogrenci universite basvuru takibi
    /// </summary>
    [HttpGet("student/{studentId}/university-tracking")]
    [ProducesResponseType(typeof(ApiResponse<StudentUniversityTrackingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<StudentUniversityTrackingDto>>> GetUniversityTracking(int studentId)
    {
        try
        {
            var result = await _dashboardService.GetStudentUniversityTrackingAsync(studentId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Universite basvuru takibi getirilirken hata: {StudentId}", studentId);
            return StatusCode(500, ApiResponse<StudentUniversityTrackingDto>.ErrorResponse("Bir hata olustu"));
        }
    }

    /// <summary>
    /// Ogrenci sinav takvimi
    /// </summary>
    [HttpGet("student/{studentId}/exam-calendar")]
    [ProducesResponseType(typeof(ApiResponse<List<ExamCalendarItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ExamCalendarItemDto>>>> GetExamCalendar(int studentId)
    {
        try
        {
            var result = await _dashboardService.GetStudentExamCalendarAsync(studentId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sinav takvimi getirilirken hata: {StudentId}", studentId);
            return StatusCode(500, ApiResponse<List<ExamCalendarItemDto>>.ErrorResponse("Bir hata olustu"));
        }
    }

    /// <summary>
    /// Ogrenci danisma gorusme notlari
    /// </summary>
    [HttpGet("student/{studentId}/notes")]
    [ProducesResponseType(typeof(ApiResponse<List<CounselorNoteDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<CounselorNoteDto>>>> GetCounselorNotes(int studentId)
    {
        try
        {
            var result = await _dashboardService.GetStudentCounselorNotesAsync(studentId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gorusme notlari getirilirken hata: {StudentId}", studentId);
            return StatusCode(500, ApiResponse<List<CounselorNoteDto>>.ErrorResponse("Bir hata olustu"));
        }
    }

    /// <summary>
    /// Yeni gorusme notu olustur
    /// </summary>
    [HttpPost("notes")]
    [ProducesResponseType(typeof(ApiResponse<CounselorNoteDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CounselorNoteDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CounselorNoteDto>>> CreateNote([FromBody] CreateCounselorNoteDto dto)
    {
        try
        {
            var counselorId = await GetCurrentCounselorIdAsync();
            if (counselorId == null)
            {
                return BadRequest(ApiResponse<CounselorNoteDto>.ErrorResponse("Danisman bilgisi bulunamadi"));
            }

            var result = await _dashboardService.CreateCounselorNoteAsync(counselorId.Value, dto);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return CreatedAtAction(nameof(GetCounselorNotes), new { studentId = dto.StudentId }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gorusme notu olusturulurken hata");
            return StatusCode(500, ApiResponse<CounselorNoteDto>.ErrorResponse("Bir hata olustu"));
        }
    }

    /// <summary>
    /// Gorusme notu guncelle
    /// </summary>
    [HttpPut("notes/{noteId}")]
    [ProducesResponseType(typeof(ApiResponse<CounselorNoteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CounselorNoteDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CounselorNoteDto>>> UpdateNote(int noteId, [FromBody] UpdateCounselorNoteDto dto)
    {
        try
        {
            if (noteId != dto.Id)
            {
                return BadRequest(ApiResponse<CounselorNoteDto>.ErrorResponse("ID uyusmazligi"));
            }

            var counselorId = await GetCurrentCounselorIdAsync();
            if (counselorId == null)
            {
                return BadRequest(ApiResponse<CounselorNoteDto>.ErrorResponse("Danisman bilgisi bulunamadi"));
            }

            var result = await _dashboardService.UpdateCounselorNoteAsync(counselorId.Value, dto);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gorusme notu guncellenirken hata: {NoteId}", noteId);
            return StatusCode(500, ApiResponse<CounselorNoteDto>.ErrorResponse("Bir hata olustu"));
        }
    }

    /// <summary>
    /// Gorusme notu sil
    /// </summary>
    [HttpDelete("notes/{noteId}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteNote(int noteId)
    {
        try
        {
            var result = await _dashboardService.DeleteCounselorNoteAsync(noteId);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gorusme notu silinirken hata: {NoteId}", noteId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Bir hata olustu"));
        }
    }

    /// <summary>
    /// Ogrenci yaklasan deadline'lari (7 gun icinde)
    /// </summary>
    [HttpGet("student/{studentId}/upcoming-deadlines")]
    [ProducesResponseType(typeof(ApiResponse<List<UpcomingDeadlineDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<UpcomingDeadlineDto>>>> GetUpcomingDeadlines(int studentId)
    {
        try
        {
            var result = await _dashboardService.GetUpcomingDeadlinesAsync(studentId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yaklasan deadline'lar getirilirken hata: {StudentId}", studentId);
            return StatusCode(500, ApiResponse<List<UpcomingDeadlineDto>>.ErrorResponse("Bir hata olustu"));
        }
    }

    /// <summary>
    /// Ogrenci dashboard ozeti
    /// </summary>
    [HttpGet("student/{studentId}/summary")]
    [ProducesResponseType(typeof(ApiResponse<CounselorDashboardSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CounselorDashboardSummaryDto>>> GetDashboardSummary(int studentId)
    {
        try
        {
            var result = await _dashboardService.GetDashboardSummaryAsync(studentId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dashboard ozeti getirilirken hata: {StudentId}", studentId);
            return StatusCode(500, ApiResponse<CounselorDashboardSummaryDto>.ErrorResponse("Bir hata olustu"));
        }
    }

    /// <summary>
    /// Danismanin tum ogrencilerinin ozeti
    /// </summary>
    [HttpGet("my-students")]
    [ProducesResponseType(typeof(ApiResponse<List<CounselorDashboardSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<CounselorDashboardSummaryDto>>>> GetMyStudentsSummary()
    {
        try
        {
            var counselorId = await GetCurrentCounselorIdAsync();
            if (counselorId == null)
            {
                return BadRequest(ApiResponse<List<CounselorDashboardSummaryDto>>.ErrorResponse("Danisman bilgisi bulunamadi"));
            }

            var result = await _dashboardService.GetCounselorStudentsSummaryAsync(counselorId.Value);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ogrenci listesi getirilirken hata");
            return StatusCode(500, ApiResponse<List<CounselorDashboardSummaryDto>>.ErrorResponse("Bir hata olustu"));
        }
    }

    #region Helper Methods

    private async Task<int?> GetCurrentCounselorIdAsync()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        var counselor = await _context.Counselors
            .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);

        return counselor?.Id;
    }

    #endregion
}
