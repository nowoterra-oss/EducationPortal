using System.Security.Claims;
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
    private readonly IAdvisorAccessService _advisorAccessService;
    private readonly IParentAccessService _parentAccessService;
    private readonly ILogger<HomeworkAssignmentsController> _logger;

    public HomeworkAssignmentsController(
        IHomeworkAssignmentService service,
        IAdvisorAccessService advisorAccessService,
        IParentAccessService parentAccessService,
        ILogger<HomeworkAssignmentsController> logger)
    {
        _service = service;
        _advisorAccessService = advisorAccessService;
        _parentAccessService = parentAccessService;
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
    /// <remarks>
    /// Danışmanlar sadece kendilerine atanmış öğrencilerin ödevlerini görüntüleyebilir.
    /// Veliler sadece kendilerine bağlı öğrencilerin ödevlerini görüntüleyebilir.
    /// </remarks>
    [HttpGet("student/{studentId}")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<PagedResponse<HomeworkAssignmentDto>>>> GetStudentAssignments(
        int studentId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            // Veli erişim kontrolü
            var isParent = await _parentAccessService.IsParentAsync(userId);
            if (isParent)
            {
                var canAccessAsParent = await _parentAccessService.CanAccessStudentAsync(userId, studentId);
                if (!canAccessAsParent)
                {
                    return StatusCode(StatusCodes.Status403Forbidden,
                        ApiResponse<PagedResponse<HomeworkAssignmentDto>>.ErrorResponse("Bu öğrencinin verilerine erişim yetkiniz bulunmamaktadır."));
                }
            }
            else
            {
                // Danışman erişim kontrolü
                var canAccess = await _advisorAccessService.CanAccessStudentAsync(userId, studentId);
                if (!canAccess)
                {
                    return StatusCode(StatusCodes.Status403Forbidden,
                        ApiResponse<PagedResponse<HomeworkAssignmentDto>>.ErrorResponse("Bu öğrencinin verilerine erişim yetkiniz bulunmamaktadır."));
                }
            }
        }

        var result = await _service.GetStudentAssignmentsAsync(studentId, pageNumber, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Öğrencinin ödev ilerleme durumlarını getir
    /// </summary>
    /// <remarks>
    /// Öğrenci kendi ödev ilerlemelerini yetki olmadan görüntüleyebilir.
    /// Veliler sadece kendilerine bağlı öğrencilerin ödev ilerlemelerini görüntüleyebilir.
    /// Danışmanlar sadece kendilerine atanmış öğrencilerin ödev ilerlemelerini görüntüleyebilir.
    /// </remarks>
    [HttpGet("student/{studentId}/progress")]
    [ProducesResponseType(typeof(ApiResponse<List<HomeworkProgressDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<HomeworkProgressDto>>>> GetStudentProgress(int studentId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            // Veli erişim kontrolü
            var isParent = await _parentAccessService.IsParentAsync(userId);
            if (isParent)
            {
                var canAccessAsParent = await _parentAccessService.CanAccessStudentAsync(userId, studentId);
                if (!canAccessAsParent)
                {
                    return StatusCode(StatusCodes.Status403Forbidden,
                        ApiResponse<List<HomeworkProgressDto>>.ErrorResponse("Bu öğrencinin verilerine erişim yetkiniz bulunmamaktadır."));
                }
            }
            else
            {
                // Danışman erişim kontrolü
                var canAccess = await _advisorAccessService.CanAccessStudentAsync(userId, studentId);
                if (!canAccess)
                {
                    return StatusCode(StatusCodes.Status403Forbidden,
                        ApiResponse<List<HomeworkProgressDto>>.ErrorResponse("Bu öğrencinin verilerine erişim yetkiniz bulunmamaktadır."));
                }
            }
        }

        var result = await _service.GetStudentProgressAsync(studentId);
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
        // DTO'dan gelen TeacherId varsa onu kullan, yoksa token'dan al
        var teacherId = dto.TeacherId ?? GetCurrentTeacherId();
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
        // DTO'dan gelen TeacherId varsa onu kullan, yoksa token'dan al
        var teacherId = dto.TeacherId ?? GetCurrentTeacherId();
        var result = await _service.CreateBulkAssignmentsAsync(teacherId, dto);
        return Ok(result);
    }

    /// <summary>
    /// Öğrencinin ödev geçmişini getir
    /// </summary>
    /// <remarks>
    /// teacherId parametresi verilmezse tüm öğretmenlerin ödevleri döndürülür.
    /// teacherId parametresi verilirse sadece o öğretmenin ödevleri döndürülür.
    /// Danışmanlar sadece kendilerine atanmış öğrencilerin ödev geçmişini görüntüleyebilir.
    /// Veliler sadece kendi çocuklarının ödev geçmişini görüntüleyebilir.
    /// </remarks>
    [HttpGet("student/{studentId}/history")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<HomeworkAssignmentDto>>>> GetStudentHistory(
        int studentId, [FromQuery] int? teacherId = null)
    {
        // Erişim kontrolü
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            // Veli erişim kontrolü
            var isParent = await _parentAccessService.IsParentAsync(userId);
            if (isParent)
            {
                var canAccessAsParent = await _parentAccessService.CanAccessStudentAsync(userId, studentId);
                if (!canAccessAsParent)
                {
                    return StatusCode(StatusCodes.Status403Forbidden,
                        ApiResponse<List<HomeworkAssignmentDto>>.ErrorResponse("Bu öğrencinin verilerine erişim yetkiniz bulunmamaktadır."));
                }
            }
            else
            {
                // Danışman erişim kontrolü
                var canAccess = await _advisorAccessService.CanAccessStudentAsync(userId, studentId);
                if (!canAccess)
                {
                    return StatusCode(StatusCodes.Status403Forbidden,
                        ApiResponse<List<HomeworkAssignmentDto>>.ErrorResponse("Bu öğrencinin verilerine erişim yetkiniz bulunmamaktadır."));
                }
            }
        }

        // teacherId verilmezse null geçir, tüm öğretmenlerin ödevleri döner
        var result = await _service.GetStudentAssignmentHistoryAsync(studentId, teacherId);
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
    /// <remarks>
    /// Admin için: teacherId parametresi verilmezse tüm öğretmenlerin ödevleri döner.
    /// Öğretmen için: Kendi ödevleri döner.
    /// status parametreleri: null (varsayılan-TeslimEdildi), "all" (tüm aktif), "pending" (henüz teslim edilmemiş)
    /// </remarks>
    [HttpGet("pending-reviews")]
    [Authorize(Roles = "Admin,Ogretmen")]
    public async Task<ActionResult<ApiResponse<PagedResponse<HomeworkAssignmentDto>>>> GetPendingReviews(
        [FromQuery] int? teacherId = null,
        [FromQuery] string? status = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        // Admin ise ve teacherId verilmemişse null geçir (tüm öğretmenlerin ödevleri)
        // Öğretmen ise kendi teacherId'sini kullan
        int? effectiveTeacherId = teacherId;
        if (!User.IsInRole("Admin") && !teacherId.HasValue)
        {
            effectiveTeacherId = GetCurrentTeacherId();
        }

        var result = await _service.GetPendingReviewsAsync(effectiveTeacherId, pageNumber, pageSize, status);
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
    /// <remarks>
    /// Admin tüm ödevleri değerlendirebilir.
    /// Öğretmen sadece kendi ödevlerini değerlendirebilir.
    /// </remarks>
    [HttpPut("{id}/grade")]
    [Authorize(Roles = "Admin,Ogretmen")]
    public async Task<ActionResult<ApiResponse<HomeworkAssignmentDto>>> GradeAssignment(
        int id, [FromBody] GradeHomeworkDto dto)
    {
        dto.AssignmentId = id;
        var isAdmin = User.IsInRole("Admin");
        int? teacherId = isAdmin ? null : GetCurrentTeacherId();
        var result = await _service.GradeAssignmentAsync(teacherId, dto, isAdmin);
        return Ok(result);
    }

    /// <summary>
    /// Öğrenci performansı
    /// </summary>
    /// <remarks>
    /// Danışmanlar sadece kendilerine atanmış öğrencilerin performansını görüntüleyebilir.
    /// Veliler sadece kendi çocuklarının performansını görüntüleyebilir.
    /// </remarks>
    [HttpGet("student/{studentId}/performance")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<StudentHomeworkPerformanceDto>>> GetStudentPerformance(
        int studentId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        // Erişim kontrolü
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            // Veli erişim kontrolü
            var isParent = await _parentAccessService.IsParentAsync(userId);
            if (isParent)
            {
                var canAccessAsParent = await _parentAccessService.CanAccessStudentAsync(userId, studentId);
                if (!canAccessAsParent)
                {
                    return StatusCode(StatusCodes.Status403Forbidden,
                        ApiResponse<StudentHomeworkPerformanceDto>.ErrorResponse("Bu öğrencinin verilerine erişim yetkiniz bulunmamaktadır."));
                }
            }
            else
            {
                // Danışman erişim kontrolü
                var canAccess = await _advisorAccessService.CanAccessStudentAsync(userId, studentId);
                if (!canAccess)
                {
                    return StatusCode(StatusCodes.Status403Forbidden,
                        ApiResponse<StudentHomeworkPerformanceDto>.ErrorResponse("Bu öğrencinin verilerine erişim yetkiniz bulunmamaktadır."));
                }
            }
        }

        var result = await _service.GetStudentPerformanceAsync(studentId, startDate, endDate);
        return Ok(result);
    }

    /// <summary>
    /// Performans chart verisi
    /// </summary>
    /// <remarks>
    /// Danışmanlar sadece kendilerine atanmış öğrencilerin performans grafiğini görüntüleyebilir.
    /// Veliler sadece kendi çocuklarının performans grafiğini görüntüleyebilir.
    /// </remarks>
    [HttpGet("student/{studentId}/performance/chart")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<HomeworkPerformanceChartDto>>> GetPerformanceChart(
        int studentId, [FromQuery] int months = 6)
    {
        // Erişim kontrolü
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            // Veli erişim kontrolü
            var isParent = await _parentAccessService.IsParentAsync(userId);
            if (isParent)
            {
                var canAccessAsParent = await _parentAccessService.CanAccessStudentAsync(userId, studentId);
                if (!canAccessAsParent)
                {
                    return StatusCode(StatusCodes.Status403Forbidden,
                        ApiResponse<HomeworkPerformanceChartDto>.ErrorResponse("Bu öğrencinin verilerine erişim yetkiniz bulunmamaktadır."));
                }
            }
            else
            {
                // Danışman erişim kontrolü
                var canAccess = await _advisorAccessService.CanAccessStudentAsync(userId, studentId);
                if (!canAccess)
                {
                    return StatusCode(StatusCodes.Status403Forbidden,
                        ApiResponse<HomeworkPerformanceChartDto>.ErrorResponse("Bu öğrencinin verilerine erişim yetkiniz bulunmamaktadır."));
                }
            }
        }

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
    /// Test teslim et (ödev teslim edildikten sonra)
    /// </summary>
    /// <remarks>
    /// Sadece TeslimEdildi durumundaki ödevler için test teslim edilebilir.
    /// </remarks>
    [HttpPost("{id}/submit-test")]
    public async Task<ActionResult<ApiResponse<HomeworkAssignmentDto>>> SubmitTest(
        int id, [FromBody] SubmitTestDto dto)
    {
        var studentId = GetCurrentStudentId();
        var result = await _service.SubmitTestAsync(id, studentId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Ödev için dosya yükle (belirli assignment'a bağlı)
    /// </summary>
    [HttpPost("{id}/upload")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB
    public async Task<ActionResult<ApiResponse<FileUploadResultDto>>> UploadFileForAssignment(
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
    /// Genel dosya yükleme endpoint'i (öğrenci ödev teslimi için).
    /// Öğrenci önce bu endpoint ile dosya yükler, dönen URL'i submit ile gönderir.
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50MB
    public async Task<ActionResult<ApiResponse<FileUploadResultDto>>> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<FileUploadResultDto>.ErrorResponse("Dosya seçilmedi"));

        using var stream = file.OpenReadStream();
        var result = await _service.UploadFileAsync(stream, file.FileName, file.ContentType);
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
