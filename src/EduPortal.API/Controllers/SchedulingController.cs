using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Scheduling;
using EduPortal.Application.Interfaces;
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

    public SchedulingController(ISchedulingService schedulingService)
    {
        _schedulingService = schedulingService;
    }

    // ===============================================
    // STUDENT CALENDAR & AVAILABILITY
    // ===============================================

    /// <summary>
    /// Öğrencinin haftalık takvimini getir
    /// </summary>
    [HttpGet("student/{studentId}/calendar")]
    [ProducesResponseType(typeof(ApiResponse<WeeklyCalendarDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<WeeklyCalendarDto>>> GetStudentCalendar(int studentId)
    {
        var result = await _schedulingService.GetStudentWeeklyCalendarAsync(studentId);
        return Ok(result);
    }

    /// <summary>
    /// Öğrencinin müsaitlik bilgilerini getir
    /// </summary>
    [HttpGet("student/{studentId}/availability")]
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
    [Authorize(Roles = "Admin,Kayitci,Öğrenci")]
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
    [Authorize(Roles = "Admin,Kayitci,Öğrenci")]
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
    [ProducesResponseType(typeof(ApiResponse<WeeklyCalendarDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<WeeklyCalendarDto>>> GetTeacherCalendar(int teacherId)
    {
        var result = await _schedulingService.GetTeacherWeeklyCalendarAsync(teacherId);
        return Ok(result);
    }

    /// <summary>
    /// Öğretmenin müsaitlik bilgilerini getir
    /// </summary>
    [HttpGet("teacher/{teacherId}/availability")]
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
    [Authorize(Roles = "Admin,Kayitci,Öğretmen")]
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
    [Authorize(Roles = "Admin,Kayitci,Öğretmen")]
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
    [ProducesResponseType(typeof(ApiResponse<List<LessonScheduleDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<LessonScheduleDto>>>> GetTeacherLessons(int teacherId)
    {
        var result = await _schedulingService.GetTeacherLessonsAsync(teacherId);
        return Ok(result);
    }

    // ===============================================
    // MATCHING & LESSON SCHEDULING
    // ===============================================

    /// <summary>
    /// Öğrenci ve öğretmenin müsait zamanlarını bul
    /// </summary>
    [HttpGet("match")]
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
    [Authorize(Roles = "Admin,Kayitci")]
    [ProducesResponseType(typeof(ApiResponse<LessonScheduleDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<LessonScheduleDto>>> CreateLesson([FromBody] CreateLessonScheduleDto dto)
    {
        // Debug log - gelen verileri kontrol et
        Console.WriteLine($"[DEBUG] CreateLesson - StudentId: {dto.StudentId}, TeacherId: {dto.TeacherId}, " +
            $"DayOfWeek: {dto.DayOfWeek}, StartTime: {dto.StartTime}, EndTime: {dto.EndTime}, " +
            $"EffectiveFrom: {dto.EffectiveFrom}, EffectiveTo: {dto.EffectiveTo}");

        var result = await _schedulingService.CreateLessonScheduleAsync(dto);
        if (result.Success)
            return CreatedAtAction(nameof(GetStudentCalendar), new { studentId = dto.StudentId }, result);
        return BadRequest(result);
    }

    /// <summary>
    /// Dersi iptal et
    /// </summary>
    [HttpPatch("lesson/{lessonId}/cancel")]
    [Authorize(Roles = "Admin,Kayitci")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> CancelLesson(int lessonId)
    {
        var result = await _schedulingService.CancelLessonAsync(lessonId);
        return Ok(result);
    }
}
