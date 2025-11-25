using EduPortal.Application.Common;
using EduPortal.Application.DTOs.AcademicTerm;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Akademik dönem yönetimi endpoint'leri
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class AcademicTermsController : ControllerBase
{
    private readonly IAcademicTermService _academicTermService;
    private readonly ILogger<AcademicTermsController> _logger;

    public AcademicTermsController(IAcademicTermService academicTermService, ILogger<AcademicTermsController> logger)
    {
        _academicTermService = academicTermService;
        _logger = logger;
    }

    /// <summary>
    /// Tüm akademik dönemleri listele
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<AcademicTermDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<AcademicTermDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var (items, totalCount) = await _academicTermService.GetAllPagedAsync(pageNumber, pageSize);
            var response = new PagedResponse<AcademicTermDto>(items.ToList(), pageNumber, pageSize, totalCount);
            return Ok(ApiResponse<PagedResponse<AcademicTermDto>>.SuccessResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving academic terms");
            return StatusCode(500, ApiResponse<PagedResponse<AcademicTermDto>>.ErrorResponse("Akademik dönemler alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// ID ile dönem detayı getir
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<AcademicTermDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AcademicTermDto>>> GetById(int id)
    {
        try
        {
            var term = await _academicTermService.GetByIdAsync(id);
            if (term == null)
                return NotFound(ApiResponse<AcademicTermDto>.ErrorResponse("Akademik dönem bulunamadı"));

            return Ok(ApiResponse<AcademicTermDto>.SuccessResponse(term));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving academic term {TermId}", id);
            return StatusCode(500, ApiResponse<AcademicTermDto>.ErrorResponse("Akademik dönem alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Aktif akademik dönemi getir
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<AcademicTermDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AcademicTermDto>>> GetActive()
    {
        try
        {
            var term = await _academicTermService.GetCurrentAsync();
            if (term == null)
                return NotFound(ApiResponse<AcademicTermDto>.ErrorResponse("Aktif akademik dönem bulunamadı"));

            return Ok(ApiResponse<AcademicTermDto>.SuccessResponse(term));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active academic term");
            return StatusCode(500, ApiResponse<AcademicTermDto>.ErrorResponse("Aktif akademik dönem alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Yeni akademik dönem oluştur (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<AcademicTermDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AcademicTermDto>>> Create([FromBody] CreateAcademicTermDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<AcademicTermDto>.ErrorResponse("Geçersiz veri"));

            var term = await _academicTermService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = term.Id },
                ApiResponse<AcademicTermDto>.SuccessResponse(term, "Akademik dönem başarıyla oluşturuldu"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating academic term");
            return StatusCode(500, ApiResponse<AcademicTermDto>.ErrorResponse("Akademik dönem oluşturulurken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Akademik dönem bilgilerini güncelle (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<AcademicTermDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AcademicTermDto>>> Update(int id, [FromBody] UpdateAcademicTermDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<AcademicTermDto>.ErrorResponse("Geçersiz veri"));

            var term = await _academicTermService.UpdateAsync(id, dto);
            return Ok(ApiResponse<AcademicTermDto>.SuccessResponse(term, "Akademik dönem başarıyla güncellendi"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<AcademicTermDto>.ErrorResponse("Akademik dönem bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating academic term {TermId}", id);
            return StatusCode(500, ApiResponse<AcademicTermDto>.ErrorResponse("Akademik dönem güncellenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Akademik dönemi aktif et (Admin only)
    /// </summary>
    [HttpPatch("{id}/activate")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<AcademicTermDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AcademicTermDto>>> Activate(int id)
    {
        try
        {
            var term = await _academicTermService.ActivateAsync(id);
            return Ok(ApiResponse<AcademicTermDto>.SuccessResponse(term, "Akademik dönem aktif edildi"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<AcademicTermDto>.ErrorResponse("Akademik dönem bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating academic term {TermId}", id);
            return StatusCode(500, ApiResponse<AcademicTermDto>.ErrorResponse("Akademik dönem aktif edilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Akademik dönemi sil (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _academicTermService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Akademik dönem bulunamadı"));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Akademik dönem başarıyla silindi"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting academic term {TermId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Akademik dönem silinirken bir hata oluştu"));
        }
    }
}
