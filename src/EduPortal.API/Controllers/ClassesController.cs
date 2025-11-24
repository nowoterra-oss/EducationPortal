using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Class;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Sınıf/şube yönetimi endpoint'leri
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ClassesController : ControllerBase
{
    private readonly IClassService _classService;
    private readonly ILogger<ClassesController> _logger;

    public ClassesController(IClassService classService, ILogger<ClassesController> logger)
    {
        _classService = classService;
        _logger = logger;
    }

    /// <summary>
    /// Tüm sınıfları listele (sayfalı)
    /// </summary>
    /// <param name="pageNumber">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa başına kayıt (varsayılan: 10)</param>
    /// <param name="grade">Sınıf seviyesi filtresi (opsiyonel)</param>
    /// <param name="academicYear">Akademik yıl filtresi (opsiyonel)</param>
    /// <returns>Sayfalanmış sınıf listesi</returns>
    /// <response code="200">Sınıflar başarıyla getirildi</response>
    /// <response code="401">Yetkisiz erişim</response>
    [HttpGet]
    [Authorize(Roles = "Admin,Öğretmen,Kayitci")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<ClassSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PagedResponse<ClassSummaryDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? grade = null,
        [FromQuery] string? academicYear = null)
    {
        try
        {
            var (items, totalCount) = await _classService.GetAllPagedAsync(pageNumber, pageSize, grade, academicYear);

            var pagedResponse = new PagedResponse<ClassSummaryDto>(
                items.ToList(),
                totalCount,
                pageNumber,
                pageSize);

            return Ok(ApiResponse<PagedResponse<ClassSummaryDto>>.SuccessResponse(pagedResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sınıflar getirilirken hata oluştu");
            return StatusCode(500, ApiResponse<PagedResponse<ClassSummaryDto>>.ErrorResponse("Sınıflar getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// ID ile sınıf getir
    /// </summary>
    /// <param name="id">Sınıf ID</param>
    /// <returns>Sınıf detayları</returns>
    /// <response code="200">Sınıf başarıyla getirildi</response>
    /// <response code="404">Sınıf bulunamadı</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ClassDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ClassDto>>> GetById(int id)
    {
        try
        {
            var classDto = await _classService.GetByIdAsync(id);
            if (classDto == null)
                return NotFound(ApiResponse<ClassDto>.ErrorResponse("Sınıf bulunamadı"));

            return Ok(ApiResponse<ClassDto>.SuccessResponse(classDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sınıf getirilirken hata oluştu. ID: {ClassId}", id);
            return StatusCode(500, ApiResponse<ClassDto>.ErrorResponse("Sınıf getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Sınıfa kayıtlı öğrencileri listele
    /// </summary>
    /// <param name="id">Sınıf ID</param>
    /// <param name="pageNumber">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt</param>
    /// <returns>Öğrenci listesi</returns>
    /// <response code="200">Öğrenciler başarıyla getirildi</response>
    [HttpGet("{id}/students")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<ClassStudentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<ClassStudentDto>>>> GetStudents(
        int id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var (items, totalCount) = await _classService.GetStudentsPagedAsync(id, pageNumber, pageSize);

            var pagedResponse = new PagedResponse<ClassStudentDto>(
                items.ToList(),
                totalCount,
                pageNumber,
                pageSize);

            return Ok(ApiResponse<PagedResponse<ClassStudentDto>>.SuccessResponse(pagedResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sınıf öğrencileri getirilirken hata oluştu. ClassId: {ClassId}", id);
            return StatusCode(500, ApiResponse<PagedResponse<ClassStudentDto>>.ErrorResponse("Öğrenciler getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Sınıfın haftalık programını getir
    /// </summary>
    /// <param name="id">Sınıf ID</param>
    /// <returns>Haftalık ders programı</returns>
    /// <response code="200">Program başarıyla getirildi</response>
    [HttpGet("{id}/schedule")]
    [ProducesResponseType(typeof(ApiResponse<List<object>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetSchedule(int id)
    {
        // Schedule endpoint requires ISchedulingService - not in current scope
        _logger.LogInformation("GetSchedule endpoint called for class {ClassId}. Use SchedulingService for schedule management.", id);
        return Ok(ApiResponse<List<object>>.SuccessResponse(new List<object>(), "Ders programı için SchedulingService kullanın"));
    }

    /// <summary>
    /// Sınıf istatistiklerini getir
    /// </summary>
    /// <param name="id">Sınıf ID</param>
    /// <returns>Sınıf istatistikleri</returns>
    /// <response code="200">İstatistikler başarıyla getirildi</response>
    [HttpGet("{id}/statistics")]
    [ProducesResponseType(typeof(ApiResponse<ClassStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ClassStatisticsDto>>> GetStatistics(int id)
    {
        try
        {
            var statistics = await _classService.GetStatisticsAsync(id);
            if (statistics == null)
                return NotFound(ApiResponse<ClassStatisticsDto>.ErrorResponse("Sınıf bulunamadı"));

            return Ok(ApiResponse<ClassStatisticsDto>.SuccessResponse(statistics));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sınıf istatistikleri getirilirken hata oluştu. ClassId: {ClassId}", id);
            return StatusCode(500, ApiResponse<ClassStatisticsDto>.ErrorResponse("İstatistikler getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Yeni sınıf oluştur
    /// </summary>
    /// <param name="createDto">Sınıf oluşturma bilgileri</param>
    /// <returns>Oluşturulan sınıf</returns>
    /// <response code="201">Sınıf başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz veri</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Kayitci")]
    [ProducesResponseType(typeof(ApiResponse<ClassDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ClassDto>>> Create([FromBody] CreateClassDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<ClassDto>.ErrorResponse("Geçersiz veri"));

            var classDto = await _classService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = classDto.Id },
                ApiResponse<ClassDto>.SuccessResponse(classDto, "Sınıf başarıyla oluşturuldu"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sınıf oluşturulurken hata oluştu");
            return StatusCode(500, ApiResponse<ClassDto>.ErrorResponse("Sınıf oluşturulurken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Sınıf bilgilerini güncelle
    /// </summary>
    /// <param name="id">Sınıf ID</param>
    /// <param name="updateDto">Güncellenecek bilgiler</param>
    /// <returns>Güncellenmiş sınıf</returns>
    /// <response code="200">Sınıf başarıyla güncellendi</response>
    /// <response code="404">Sınıf bulunamadı</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Kayitci")]
    [ProducesResponseType(typeof(ApiResponse<ClassDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ClassDto>>> Update(int id, [FromBody] UpdateClassDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<ClassDto>.ErrorResponse("Geçersiz veri"));

            var classDto = await _classService.UpdateAsync(id, updateDto);
            return Ok(ApiResponse<ClassDto>.SuccessResponse(classDto, "Sınıf başarıyla güncellendi"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<ClassDto>.ErrorResponse("Sınıf bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sınıf güncellenirken hata oluştu. ID: {ClassId}", id);
            return StatusCode(500, ApiResponse<ClassDto>.ErrorResponse("Sınıf güncellenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Sınıfı sil (soft delete)
    /// </summary>
    /// <param name="id">Sınıf ID</param>
    /// <returns>Silme işlemi sonucu</returns>
    /// <response code="200">Sınıf başarıyla silindi</response>
    /// <response code="404">Sınıf bulunamadı</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _classService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Sınıf bulunamadı"));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Sınıf başarıyla silindi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sınıf silinirken hata oluştu. ID: {ClassId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Sınıf silinirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Belirli sınıf seviyesindeki sınıfları getir
    /// </summary>
    /// <param name="grade">Sınıf seviyesi (9, 10, 11, 12)</param>
    /// <returns>Sınıf listesi</returns>
    [HttpGet("by-grade/{grade}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ClassDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ClassDto>>>> GetByGrade(int grade)
    {
        try
        {
            var classes = await _classService.GetByGradeAsync(grade);
            return Ok(ApiResponse<IEnumerable<ClassDto>>.SuccessResponse(classes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sınıflar grade'e göre getirilirken hata oluştu. Grade: {Grade}", grade);
            return StatusCode(500, ApiResponse<IEnumerable<ClassDto>>.ErrorResponse("Sınıflar getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Belirli akademik yıldaki sınıfları getir
    /// </summary>
    /// <param name="academicYear">Akademik yıl (örn: 2024-2025)</param>
    /// <returns>Sınıf listesi</returns>
    [HttpGet("by-academic-year/{academicYear}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ClassDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ClassDto>>>> GetByAcademicYear(string academicYear)
    {
        try
        {
            var classes = await _classService.GetByAcademicYearAsync(academicYear);
            return Ok(ApiResponse<IEnumerable<ClassDto>>.SuccessResponse(classes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sınıflar akademik yıla göre getirilirken hata oluştu. AcademicYear: {AcademicYear}", academicYear);
            return StatusCode(500, ApiResponse<IEnumerable<ClassDto>>.ErrorResponse("Sınıflar getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Aktif sınıfları getir
    /// </summary>
    /// <returns>Aktif sınıf listesi</returns>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ClassDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ClassDto>>>> GetActiveClasses()
    {
        try
        {
            var classes = await _classService.GetActiveClassesAsync();
            return Ok(ApiResponse<IEnumerable<ClassDto>>.SuccessResponse(classes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Aktif sınıflar getirilirken hata oluştu");
            return StatusCode(500, ApiResponse<IEnumerable<ClassDto>>.ErrorResponse("Sınıflar getirilirken bir hata oluştu"));
        }
    }
}
