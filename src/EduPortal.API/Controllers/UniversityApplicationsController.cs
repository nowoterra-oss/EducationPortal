using EduPortal.Application.Common;
using EduPortal.Application.DTOs.UniversityApplication;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// University application management endpoints
/// </summary>
[ApiController]
[Route("api/university-applications")]
[Produces("application/json")]
[Authorize]
public class UniversityApplicationsController : ControllerBase
{
    private readonly IUniversityApplicationService _applicationService;
    private readonly ILogger<UniversityApplicationsController> _logger;

    public UniversityApplicationsController(IUniversityApplicationService applicationService, ILogger<UniversityApplicationsController> logger)
    {
        _applicationService = applicationService;
        _logger = logger;
    }

    /// <summary>
    /// Get all university applications with pagination
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Danışman")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<UniversityApplicationDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<UniversityApplicationDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var (items, totalCount) = await _applicationService.GetAllPagedAsync(pageNumber, pageSize);

            var pagedResponse = new PagedResponse<UniversityApplicationDto>(
                items.ToList(),
                totalCount,
                pageNumber,
                pageSize);

            return Ok(ApiResponse<PagedResponse<UniversityApplicationDto>>.SuccessResponse(pagedResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Başvurular getirilirken hata oluştu");
            return StatusCode(500, ApiResponse<PagedResponse<UniversityApplicationDto>>.ErrorResponse("Başvurular getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get application by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UniversityApplicationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UniversityApplicationDto>>> GetById(int id)
    {
        try
        {
            var application = await _applicationService.GetByIdAsync(id);

            if (application == null)
                return NotFound(ApiResponse<UniversityApplicationDto>.ErrorResponse("Başvuru bulunamadı"));

            return Ok(ApiResponse<UniversityApplicationDto>.SuccessResponse(application));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Başvuru getirilirken hata oluştu. ID: {ApplicationId}", id);
            return StatusCode(500, ApiResponse<UniversityApplicationDto>.ErrorResponse("Başvuru getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Create university application
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Danışman,Öğrenci")]
    [ProducesResponseType(typeof(ApiResponse<UniversityApplicationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<UniversityApplicationDto>>> Create([FromBody] CreateUniversityApplicationDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<UniversityApplicationDto>.ErrorResponse("Geçersiz veri"));

            var application = await _applicationService.CreateAsync(createDto);

            return CreatedAtAction(nameof(GetById), new { id = application.Id },
                ApiResponse<UniversityApplicationDto>.SuccessResponse(application, "Başvuru başarıyla oluşturuldu"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Başvuru oluşturulurken hata oluştu");
            return StatusCode(500, ApiResponse<UniversityApplicationDto>.ErrorResponse("Başvuru oluşturulurken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Update university application
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Danışman,Öğrenci")]
    [ProducesResponseType(typeof(ApiResponse<UniversityApplicationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UniversityApplicationDto>>> Update(int id, [FromBody] UpdateUniversityApplicationDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<UniversityApplicationDto>.ErrorResponse("Geçersiz veri"));

            var application = await _applicationService.UpdateAsync(id, updateDto);

            return Ok(ApiResponse<UniversityApplicationDto>.SuccessResponse(application, "Başvuru başarıyla güncellendi"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<UniversityApplicationDto>.ErrorResponse("Başvuru bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Başvuru güncellenirken hata oluştu. ID: {ApplicationId}", id);
            return StatusCode(500, ApiResponse<UniversityApplicationDto>.ErrorResponse("Başvuru güncellenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Delete university application
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _applicationService.DeleteAsync(id);

            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Başvuru bulunamadı"));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Başvuru başarıyla silindi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Başvuru silinirken hata oluştu. ID: {ApplicationId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Başvuru silinirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get applications for a student
    /// </summary>
    [HttpGet("student/{studentId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UniversityApplicationDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<UniversityApplicationDto>>>> GetByStudent(int studentId)
    {
        try
        {
            var applications = await _applicationService.GetByStudentAsync(studentId);
            return Ok(ApiResponse<IEnumerable<UniversityApplicationDto>>.SuccessResponse(applications));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Öğrenci başvuruları getirilirken hata oluştu. StudentId: {StudentId}", studentId);
            return StatusCode(500, ApiResponse<IEnumerable<UniversityApplicationDto>>.ErrorResponse("Başvurular getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get applications by status
    /// </summary>
    [HttpGet("status/{status}")]
    [Authorize(Roles = "Admin,Danışman")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<UniversityApplicationDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<UniversityApplicationDto>>>> GetByStatus(
        ApplicationStatus status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var (items, totalCount) = await _applicationService.GetByStatusAsync(status, pageNumber, pageSize);

            var pagedResponse = new PagedResponse<UniversityApplicationDto>(
                items.ToList(),
                totalCount,
                pageNumber,
                pageSize);

            return Ok(ApiResponse<PagedResponse<UniversityApplicationDto>>.SuccessResponse(pagedResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Duruma göre başvurular getirilirken hata oluştu. Status: {Status}", status);
            return StatusCode(500, ApiResponse<PagedResponse<UniversityApplicationDto>>.ErrorResponse("Başvurular getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Update application status
    /// </summary>
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,Danışman")]
    [ProducesResponseType(typeof(ApiResponse<UniversityApplicationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UniversityApplicationDto>>> UpdateStatus(int id, [FromBody] ApplicationStatusDto statusDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<UniversityApplicationDto>.ErrorResponse("Geçersiz veri"));

            var application = await _applicationService.UpdateStatusAsync(id, statusDto);

            return Ok(ApiResponse<UniversityApplicationDto>.SuccessResponse(application, "Başvuru durumu güncellendi"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<UniversityApplicationDto>.ErrorResponse("Başvuru bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Başvuru durumu güncellenirken hata oluştu. ID: {ApplicationId}", id);
            return StatusCode(500, ApiResponse<UniversityApplicationDto>.ErrorResponse("Durum güncellenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Add document to application
    /// </summary>
    [HttpPost("{id}/documents")]
    [Authorize(Roles = "Admin,Danışman,Öğrenci")]
    [ProducesResponseType(typeof(ApiResponse<ApplicationDocumentResultDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ApplicationDocumentResultDto>>> AddDocument(int id, [FromBody] AddApplicationDocumentDto documentDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<ApplicationDocumentResultDto>.ErrorResponse("Geçersiz veri"));

            var result = await _applicationService.AddDocumentAsync(id, documentDto);

            return CreatedAtAction(nameof(GetById), new { id },
                ApiResponse<ApplicationDocumentResultDto>.SuccessResponse(result, "Belge başarıyla eklendi"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<ApplicationDocumentResultDto>.ErrorResponse("Başvuru bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Belge eklenirken hata oluştu. ApplicationId: {ApplicationId}", id);
            return StatusCode(500, ApiResponse<ApplicationDocumentResultDto>.ErrorResponse("Belge eklenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get application timeline
    /// </summary>
    [HttpGet("{id}/timeline")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ApplicationTimelineDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ApplicationTimelineDto>>>> GetTimeline(int id)
    {
        try
        {
            var timeline = await _applicationService.GetTimelineAsync(id);
            return Ok(ApiResponse<IEnumerable<ApplicationTimelineDto>>.SuccessResponse(timeline));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<IEnumerable<ApplicationTimelineDto>>.ErrorResponse("Başvuru bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Başvuru zaman çizelgesi getirilirken hata oluştu. ID: {ApplicationId}", id);
            return StatusCode(500, ApiResponse<IEnumerable<ApplicationTimelineDto>>.ErrorResponse("Zaman çizelgesi getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get application statistics
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,Danışman")]
    [ProducesResponseType(typeof(ApiResponse<ApplicationStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ApplicationStatisticsDto>>> GetStatistics()
    {
        try
        {
            var statistics = await _applicationService.GetStatisticsAsync();
            return Ok(ApiResponse<ApplicationStatisticsDto>.SuccessResponse(statistics));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Başvuru istatistikleri getirilirken hata oluştu");
            return StatusCode(500, ApiResponse<ApplicationStatisticsDto>.ErrorResponse("İstatistikler getirilirken bir hata oluştu"));
        }
    }
}
