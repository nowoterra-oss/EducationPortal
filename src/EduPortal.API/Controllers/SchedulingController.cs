using System.Security.Claims;
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
    private readonly ITeacherRepository _teacherRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IPermissionService _permissionService;
    private readonly IParentAccessService _parentAccessService;

    public SchedulingController(
        ISchedulingService schedulingService,
        IStudentGroupService studentGroupService,
        ITeacherRepository teacherRepository,
        IStudentRepository studentRepository,
        IPermissionService permissionService,
        IParentAccessService parentAccessService)
    {
        _schedulingService = schedulingService;
        _studentGroupService = studentGroupService;
        _teacherRepository = teacherRepository;
        _studentRepository = studentRepository;
        _permissionService = permissionService;
        _parentAccessService = parentAccessService;
    }

    // ===============================================
    // STUDENT CALENDAR & AVAILABILITY
    // ===============================================

    /// <summary>
    /// Öğrencinin haftalık takvimini getir
    /// </summary>
    /// <remarks>
    /// Öğrenci kendi takvimini yetki olmadan görüntüleyebilir.
    /// Başka öğrencinin takvimini görüntülemek için scheduling.view yetkisi gereklidir.
    /// </remarks>
    [HttpGet("student/{studentId}/calendar")]
    [ProducesResponseType(typeof(ApiResponse<WeeklyCalendarDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<WeeklyCalendarDto>>> GetStudentCalendar(int studentId, [FromQuery] DateTime? weekStartDate = null)
    {
        var authResult = await CheckStudentAccessAsync(studentId);
        if (authResult != null) return authResult;

        var result = await _schedulingService.GetStudentWeeklyCalendarAsync(studentId, weekStartDate);
        return Ok(result);
    }

    /// <summary>
    /// Öğrencinin müsaitlik bilgilerini getir
    /// </summary>
    /// <remarks>
    /// Öğrenci kendi müsaitlik bilgilerini yetki olmadan görüntüleyebilir.
    /// Başka öğrencinin müsaitlik bilgilerini görüntülemek için scheduling.view yetkisi gereklidir.
    /// </remarks>
    [HttpGet("student/{studentId}/availability")]
    [ProducesResponseType(typeof(ApiResponse<List<StudentAvailabilityDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<StudentAvailabilityDto>>>> GetStudentAvailability(int studentId)
    {
        var authResult = await CheckStudentAccessAsync(studentId);
        if (authResult != null) return authResult;

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
    /// <remarks>
    /// Öğrenci kendi ders programını yetki olmadan görüntüleyebilir.
    /// Başka öğrencinin ders programını görüntülemek için scheduling.view yetkisi gereklidir.
    /// </remarks>
    [HttpGet("student/{studentId}/lessons")]
    [ProducesResponseType(typeof(ApiResponse<List<LessonScheduleDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<LessonScheduleDto>>>> GetStudentLessons(int studentId)
    {
        var authResult = await CheckStudentAccessAsync(studentId);
        if (authResult != null) return authResult;

        var result = await _schedulingService.GetStudentLessonsAsync(studentId);
        return Ok(result);
    }

    // ===============================================
    // TEACHER CALENDAR & AVAILABILITY
    // ===============================================

    /// <summary>
    /// Öğretmenin haftalık takvimini getir
    /// </summary>
    /// <remarks>
    /// Öğretmen kendi takvimini yetki olmadan görüntüleyebilir.
    /// Başka öğretmenin takvimini görüntülemek için scheduling.view yetkisi gereklidir.
    /// </remarks>
    [HttpGet("teacher/{teacherId}/calendar")]
    [ProducesResponseType(typeof(ApiResponse<WeeklyCalendarDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<WeeklyCalendarDto>>> GetTeacherCalendar(int teacherId, [FromQuery] DateTime? weekStartDate = null)
    {
        var authResult = await CheckTeacherAccessAsync(teacherId);
        if (authResult != null) return authResult;

        var result = await _schedulingService.GetTeacherWeeklyCalendarAsync(teacherId, weekStartDate);
        return Ok(result);
    }

    /// <summary>
    /// Öğretmenin müsaitlik bilgilerini getir
    /// </summary>
    /// <remarks>
    /// Öğretmen kendi müsaitlik bilgilerini yetki olmadan görüntüleyebilir.
    /// Başka öğretmenin müsaitlik bilgilerini görüntülemek için scheduling.view yetkisi gereklidir.
    /// </remarks>
    [HttpGet("teacher/{teacherId}/availability")]
    [ProducesResponseType(typeof(ApiResponse<List<TeacherAvailabilityDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<TeacherAvailabilityDto>>>> GetTeacherAvailability(int teacherId)
    {
        var authResult = await CheckTeacherAccessAsync(teacherId);
        if (authResult != null) return authResult;

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
    /// <remarks>
    /// Öğretmen kendi ders programını yetki olmadan görüntüleyebilir.
    /// Başka öğretmenin ders programını görüntülemek için scheduling.view yetkisi gereklidir.
    /// </remarks>
    [HttpGet("teacher/{teacherId}/lessons")]
    [ProducesResponseType(typeof(ApiResponse<List<LessonScheduleDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<LessonScheduleDto>>>> GetTeacherLessons(int teacherId)
    {
        var authResult = await CheckTeacherAccessAsync(teacherId);
        if (authResult != null) return authResult;

        var result = await _schedulingService.GetTeacherLessonsAsync(teacherId);
        return Ok(result);
    }

    /// <summary>
    /// Öğretmenin grup derslerini getir
    /// </summary>
    /// <remarks>
    /// Öğretmen kendi grup derslerini yetki olmadan görüntüleyebilir.
    /// Başka öğretmenin grup derslerini görüntülemek için scheduling.view yetkisi gereklidir.
    /// </remarks>
    [HttpGet("teacher/{teacherId}/group-lessons")]
    [ProducesResponseType(typeof(ApiResponse<List<GroupLessonScheduleDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<GroupLessonScheduleDto>>>> GetTeacherGroupLessons(int teacherId)
    {
        var authResult = await CheckTeacherAccessAsync(teacherId);
        if (authResult != null) return authResult;

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

    // ===============================================
    // HELPER METHODS
    // ===============================================

    /// <summary>
    /// Öğretmen erişim kontrolü yapar.
    /// Öğretmen kendi verilerine erişebilir, başkasının verilerine erişmek için scheduling.view yetkisi gereklidir.
    /// </summary>
    /// <param name="teacherId">Erişilmek istenen öğretmen ID</param>
    /// <returns>Erişim reddedildiyse ActionResult, aksi halde null</returns>
    private async Task<ActionResult?> CheckTeacherAccessAsync(int teacherId)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Forbid();
        }

        // Admin her zaman erişebilir
        if (User.IsInRole("Admin"))
        {
            return null;
        }

        // Öğretmeni bul
        var teacher = await _teacherRepository.GetByIdAsync(teacherId);
        if (teacher == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Öğretmen bulunamadı"));
        }

        // Kendi takvimi mi kontrol et
        bool isOwnSchedule = teacher.UserId == currentUserId;
        if (isOwnSchedule)
        {
            return null; // Kendi takvimi - yetki gerekmez
        }

        // Başka öğretmenin takvimi - scheduling.view yetkisi gerekli
        var hasPermission = await _permissionService.HasPermissionAsync(currentUserId, Permissions.SchedulingView);
        if (!hasPermission)
        {
            return Forbid();
        }

        return null;
    }

    /// <summary>
    /// Öğrenci erişim kontrolü yapar.
    /// Öğrenci kendi verilerine erişebilir, veli kendi çocuğunun verilerine erişebilir,
    /// başkasının verilerine erişmek için scheduling.view yetkisi gereklidir.
    /// </summary>
    /// <param name="studentId">Erişilmek istenen öğrenci ID</param>
    /// <returns>Erişim reddedildiyse ActionResult, aksi halde null</returns>
    private async Task<ActionResult?> CheckStudentAccessAsync(int studentId)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Forbid();
        }

        // Admin her zaman erişebilir
        if (User.IsInRole("Admin"))
        {
            return null;
        }

        // Öğrenciyi bul
        var student = await _studentRepository.GetByIdAsync(studentId);
        if (student == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Öğrenci bulunamadı"));
        }

        // Kendi verileri mi kontrol et (öğrenci)
        bool isOwnData = student.UserId == currentUserId;
        if (isOwnData)
        {
            return null; // Kendi verileri - yetki gerekmez
        }

        // Veli erişim kontrolü - kendi çocuğunun verilerine erişebilir
        var userType = User.FindFirstValue("UserType");
        if (userType == "Parent")
        {
            var canAccess = await _parentAccessService.CanAccessStudentAsync(currentUserId, studentId);
            if (canAccess)
            {
                return null; // Veli kendi çocuğunun verilerine erişebilir
            }
        }

        // Başka öğrencinin verileri - scheduling.view yetkisi gerekli
        var hasPermission = await _permissionService.HasPermissionAsync(currentUserId, Permissions.SchedulingView);
        if (!hasPermission)
        {
            return Forbid();
        }

        return null;
    }
}
