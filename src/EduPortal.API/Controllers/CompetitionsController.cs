using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Competition;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Yarışma ve ödül yönetimi endpoint'leri
/// </summary>
[ApiController]
[Route("api/competitions")]
[Produces("application/json")]
[Authorize]
public class CompetitionsController : ControllerBase
{
    private readonly ICompetitionService _competitionService;
    private readonly ILogger<CompetitionsController> _logger;

    public CompetitionsController(ICompetitionService competitionService, ILogger<CompetitionsController> logger)
    {
        _competitionService = competitionService;
        _logger = logger;
    }

    /// <summary>
    /// Tüm yarışma ve ödülleri listele
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<CompetitionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<CompetitionDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var (items, totalCount) = await _competitionService.GetAllPagedAsync(pageNumber, pageSize);
            var response = new PagedResponse<CompetitionDto>(items.ToList(), pageNumber, pageSize, totalCount);
            return Ok(ApiResponse<PagedResponse<CompetitionDto>>.SuccessResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving competitions");
            return StatusCode(500, ApiResponse<PagedResponse<CompetitionDto>>.ErrorResponse("Yarışmalar alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// ID ile yarışma/ödül detayı getir
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CompetitionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CompetitionDto>>> GetById(int id)
    {
        try
        {
            var competition = await _competitionService.GetByIdAsync(id);
            if (competition == null)
                return NotFound(ApiResponse<CompetitionDto>.ErrorResponse("Yarışma/Ödül bulunamadı"));

            return Ok(ApiResponse<CompetitionDto>.SuccessResponse(competition));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving competition {CompetitionId}", id);
            return StatusCode(500, ApiResponse<CompetitionDto>.ErrorResponse("Yarışma/Ödül alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Öğrencinin yarışma ve ödüllerini getir
    /// </summary>
    [HttpGet("student/{studentId}")]
    [ProducesResponseType(typeof(ApiResponse<List<CompetitionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<CompetitionDto>>>> GetByStudent(int studentId)
    {
        try
        {
            var competitions = await _competitionService.GetByStudentAsync(studentId);
            return Ok(ApiResponse<List<CompetitionDto>>.SuccessResponse(competitions.ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving competitions for student {StudentId}", studentId);
            return StatusCode(500, ApiResponse<List<CompetitionDto>>.ErrorResponse("Öğrenci yarışmaları alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Yeni yarışma/ödül kaydı oluştur
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Danışman")]
    [ProducesResponseType(typeof(ApiResponse<CompetitionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CompetitionDto>>> Create([FromBody] CreateCompetitionDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<CompetitionDto>.ErrorResponse("Geçersiz veri"));

            var competition = await _competitionService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = competition.Id },
                ApiResponse<CompetitionDto>.SuccessResponse(competition, "Yarışma/Ödül başarıyla oluşturuldu"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CompetitionDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating competition");
            return StatusCode(500, ApiResponse<CompetitionDto>.ErrorResponse("Yarışma/Ödül oluşturulurken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Yarışma/ödül bilgilerini güncelle
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Danışman")]
    [ProducesResponseType(typeof(ApiResponse<CompetitionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CompetitionDto>>> Update(int id, [FromBody] UpdateCompetitionDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<CompetitionDto>.ErrorResponse("Geçersiz veri"));

            var competition = await _competitionService.UpdateAsync(id, dto);
            return Ok(ApiResponse<CompetitionDto>.SuccessResponse(competition, "Yarışma/Ödül başarıyla güncellendi"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<CompetitionDto>.ErrorResponse("Yarışma/Ödül bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating competition {CompetitionId}", id);
            return StatusCode(500, ApiResponse<CompetitionDto>.ErrorResponse("Yarışma/Ödül güncellenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Yarışma/ödül kaydını sil
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _competitionService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Yarışma/Ödül bulunamadı"));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Yarışma/Ödül başarıyla silindi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting competition {CompetitionId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Yarışma/Ödül silinirken bir hata oluştu"));
        }
    }
}
