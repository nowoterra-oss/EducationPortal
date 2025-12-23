using EduPortal.API.Attributes;
using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Scheduling;
using EduPortal.Application.DTOs.StudentGroup;
using EduPortal.Application.Interfaces;
using EduPortal.Application.Services.Interfaces;
using EduPortal.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/scheduling")]
[Produces("application/json")]
[Authorize]
public class SchedulingController : ControllerBase
{
    private readonly ISchedulingService _schedulingService;
    private readonly IStudentGroupService _studentGroupService;

    public SchedulingController(ISchedulingService schedulingService, IStudentGroupService studentGroupService)
    {
        _schedulingService = schedulingService;
        _studentGroupService = studentGroupService;
    }

    // ===============================================
    // STUDENT CALENDAR & AVAILABILITY
    // ===============================================

    /// <summary>
    /// Öğrencinin haftalık takvimini getir
    /// </summary>
    [HttpGet("student/{studentId}/calendar")]
    [RequirePermission(Permissions.SchedulingView)]
    [ProducesResponseType(typeof(ApiResponse<WeeklyCalendarDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<WeeklyCalendarDto>>> GetStudentCalendar(int studentId, [FromQuery] DateTime? weekStartDate = null)
    {
        var result = await _schedulingService.GetStudentWeeklyCalendarAsync(studentId, weekStartDate);
        return Ok(result);
    }

    /// <summary>
    /// Öğrencinin müsaitlik bilgilerini getir
    /// </summary>
    [HttpGet("student/{studentId}/availability")]
    [RequirePermission(Permissions.SchedulingView)]
    [ProducesResponseType(typeof(ApiResponse<List<StudentAvailabilityDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<StudentAvailabilityDto>>>> GetStudentAvailability(int studentId)
    {
        var result = await _schedulingService.GetStudentAvailabilityAsync(studentId);
        return Ok(result);
    }

    /// <summary>
    /// Öğrenci müsaitlik ekle
    /// </summary>
    [HttpPost("student/availability")]
    [RequirePermission(Permissions.SchedulingCreate)]
    [ProducesResponseType(typeof(ApiResponse<StudentAvailabilityDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<StudentAvailabilityDto>>> CreateStudentAvailability([FromBody] CreateStudentAvailabilityDto dto)
    {
        var result = await _schedulingService.CreateStudentAvailabilityAsync(dto);
        return result.Success
            ? CreatedAtAction(nameof(GetStudentCalendar), new { studentId = dto.StudentId }, result)
            : BadRequest(result);
    }

    /// <summary>
    /// Öğrenci müsaitlik sil
    /// </summary>
    [HttpDelete("student/availability/{id}")]
    [RequirePermission(Permissions.SchedulingEdit)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteStudentAvailability(int id)
    {
        var result = await _schedulingService.DeleteStudentAvailabilityAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Öğrencinin ders programını getir
    /// </summary>
    [HttpGet("student/{studentId}/lessons")]
    [RequirePermission(Permissions.SchedulingView)]
    [ProducesResponseType(typeof(ApiResponse<List<LessonScheduleDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<LessonScheduleDto>>>> GetStudentLessons(int studentId)
    {
        var result = await _schedulingService.GetStudentLessonsAsync(studentId);
        return Ok(result);
    }

    // ===============================================
    // TEACHER CALENDAR & AVAILABILITY
    // ===============================================

    /// <summary>
    /// Öğretmenin haftalık takvimini getir
    /// </summary>
    [HttpGet("teacher/{teacherId}/calendar")]
    [RequirePermission(Permissions.SchedulingView)]
    [ProducesResponseType(typeof(ApiResponse<WeeklyCalendarDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<WeeklyCalendarDto>>> GetTeacherCalendar(int teacherId, [FromQuery] DateTime? weekStartDate = null)
    {
        var result = await _schedulingService.GetTeacherWeeklyCalendarAsync(teacherId, weekStartDate);
        return Ok(result);
    }

    /// <summary>
    /// Öğretmenin müsaitlik bilgilerini getir
    /// </summary>
    [HttpGet("teacher/{teacherId}/availability")]
    [RequirePermission(Permissions.SchedulingView)]
    [ProducesResponseType(typeof(ApiResponse<List<TeacherAvailabilityDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<TeacherAvailabilityDto>>>> GetTeacherAvailability(int teacherId)
    {
        var result = await _schedulingService.GetTeacherAvailabilityAsync(teacherId);
        return Ok(result);
    }

    /// <summary>
    /// Öğretmen müsaitlik ekle
    /// </summary>
    [HttpPost("teacher/availability")]
    [RequirePermission(Permissions.SchedulingCreate)]
    [ProducesResponseType(typeof(ApiResponse<TeacherAvailabilityDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<TeacherAvailabilityDto>>> CreateTeacherAvailability([FromBody] CreateTeacherAvailabilityDto dto)
    {
        var result = await _schedulingService.CreateTeacherAvailabilityAsync(dto);
        return result.Success
            ? CreatedAtAction(nameof(GetTeacherCalendar), new { teacherId = dto.TeacherId }, result)
            : BadRequest(result);
    }

    /// <summary>
    /// Öğretmen müsaitlik sil
    /// </summary>
    [HttpDelete("teacher/availability/{id}")]
    [RequirePermission(Permissions.SchedulingEdit)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTeacherAvailability(int id)
    {
        var result = await _schedulingService.DeleteTeacherAvailabilityAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Öğretmenin ders programını getir
    /// </summary>
    [HttpGet("teacher/{teacherId}/lessons")]
    [RequirePermission(Permissions.SchedulingView)]
    [ProducesResponseType(typeof(ApiResponse<List<LessonScheduleDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<LessonScheduleDto>>>> GetTeacherLessons(int teacherId)
    {
        var result = await _schedulingService.GetTeacherLessonsAsync(teacherId);
        return Ok(result);
    }

    /// <summary>
    /// Öğretmenin grup derslerini getir
    /// </summary>
    [HttpGet("teacher/{teacherId}/group-lessons")]
    [RequirePermission(Permissions.SchedulingView)]
    [ProducesResponseType(typeof(ApiResponse<List<GroupLessonScheduleDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<GroupLessonScheduleDto>>>> GetTeacherGroupLessons(int teacherId)
    {
        var result = await _studentGroupService.GetTeacherGroupLessonsAsync(teacherId);
        return Ok(result);
    }

    // ===============================================
    // MATCHING & LESSON SCHEDULING
    // ===============================================

    /// <summary>
    /// Öğrenci ve öğretmenin müsait zamanlarını bul
    /// </summary>
    [HttpGet("match")]
    [RequirePermission(Permissions.SchedulingView)]
    [ProducesResponseType(typeof(ApiResponse<MatchingResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<MatchingResultDto>>> FindMatchingSlots(
        [FromQuery] int studentId,
        [FromQuery] int teacherId,
        [FromQuery] DayOfWeek? dayOfWeek = null)
    {
        var result = await _schedulingService.FindMatchingSlotsAsync(studentId, teacherId, dayOfWeek);
        return Ok(result);
    }

    /// <summary>
    /// Yeni ders programı oluştur
    /// </summary>
    [HttpPost("lesson")]
    [RequirePermission(Permissions.SchedulingCreate)]
    [ProducesResponseType(typeof(ApiResponse<LessonScheduleDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<LessonScheduleDto>>> CreateLesson([FromBody] CreateLessonScheduleDto dto)
    {
        var result = await _schedulingService.CreateLessonScheduleAsync(dto);
        if (result.Success)
            return CreatedAtAction(nameof(GetStudentCalendar), new { studentId = dto.StudentId }, result);
        return BadRequest(result);
    }

    /// <summary>
    /// Dersi iptal et
    /// </summary>
    /// <param name="lessonId">Ders ID</param>
    /// <param name="cancelAll">true: Tüm tekrarları iptal et, false: Sadece belirtilen tarihi iptal et</param>
    /// <param name="cancelDate">İptal edilecek tarih (cancelAll=false ise zorunlu)</param>
    [HttpPatch("lesson/{lessonId}/cancel")]
    [RequirePermission(Permissions.SchedulingEdit)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> CancelLesson(
        int lessonId,
        [FromQuery] bool cancelAll = true,
        [FromQuery] DateTime? cancelDate = null)
    {
        var result = await _schedulingService.CancelLessonAsync(lessonId, cancelAll, cancelDate);
        return Ok(result);
    }
}
