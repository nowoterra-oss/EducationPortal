using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Hobby;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Hobi yönetimi endpoint'leri
/// </summary>
[ApiController]
[Route("api/hobbies")]
[Produces("application/json")]
[Authorize]
public class HobbiesController : ControllerBase
{
    private readonly IHobbyService _hobbyService;
    private readonly ILogger<HobbiesController> _logger;

    public HobbiesController(IHobbyService hobbyService, ILogger<HobbiesController> logger)
    {
        _hobbyService = hobbyService;
        _logger = logger;
    }

    /// <summary>
    /// Tüm hobileri listele
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<HobbyDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<HobbyDto>>>> GetAll()
    {
        try
        {
            var hobbies = await _hobbyService.GetAllAsync();
            return Ok(ApiResponse<List<HobbyDto>>.SuccessResponse(hobbies.ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving hobbies");
            return StatusCode(500, ApiResponse<List<HobbyDto>>.ErrorResponse("Hobiler alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// ID ile hobi detayı getir
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<HobbyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HobbyDto>>> GetById(int id)
    {
        try
        {
            var hobby = await _hobbyService.GetByIdAsync(id);
            if (hobby == null)
                return NotFound(ApiResponse<HobbyDto>.ErrorResponse("Hobi bulunamadı"));

            return Ok(ApiResponse<HobbyDto>.SuccessResponse(hobby));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving hobby {HobbyId}", id);
            return StatusCode(500, ApiResponse<HobbyDto>.ErrorResponse("Hobi alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Öğrencinin hobilerini getir
    /// </summary>
    [HttpGet("student/{studentId}")]
    [ProducesResponseType(typeof(ApiResponse<List<HobbyDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<HobbyDto>>>> GetByStudent(int studentId)
    {
        try
        {
            var hobbies = await _hobbyService.GetByStudentAsync(studentId);
            return Ok(ApiResponse<List<HobbyDto>>.SuccessResponse(hobbies.ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving hobbies for student {StudentId}", studentId);
            return StatusCode(500, ApiResponse<List<HobbyDto>>.ErrorResponse("Öğrenci hobileri alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Kategoriye göre hobileri getir
    /// </summary>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(ApiResponse<List<HobbyDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<HobbyDto>>>> GetByCategory(string category)
    {
        try
        {
            var hobbies = await _hobbyService.GetByCategoryAsync(category);
            return Ok(ApiResponse<List<HobbyDto>>.SuccessResponse(hobbies.ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving hobbies for category {Category}", category);
            return StatusCode(500, ApiResponse<List<HobbyDto>>.ErrorResponse("Kategori hobileri alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Yeni hobi oluştur
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<HobbyDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<HobbyDto>>> Create([FromBody] CreateHobbyDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<HobbyDto>.ErrorResponse("Geçersiz veri"));

            var hobby = await _hobbyService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = hobby.Id },
                ApiResponse<HobbyDto>.SuccessResponse(hobby, "Hobi başarıyla oluşturuldu"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<HobbyDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating hobby");
            return StatusCode(500, ApiResponse<HobbyDto>.ErrorResponse("Hobi oluşturulurken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Hobi bilgilerini güncelle
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<HobbyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HobbyDto>>> Update(int id, [FromBody] UpdateHobbyDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<HobbyDto>.ErrorResponse("Geçersiz veri"));

            var hobby = await _hobbyService.UpdateAsync(id, dto);
            return Ok(ApiResponse<HobbyDto>.SuccessResponse(hobby, "Hobi başarıyla güncellendi"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<HobbyDto>.ErrorResponse("Hobi bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating hobby {HobbyId}", id);
            return StatusCode(500, ApiResponse<HobbyDto>.ErrorResponse("Hobi güncellenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Hobiyi sil
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _hobbyService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Hobi bulunamadı"));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Hobi başarıyla silindi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting hobby {HobbyId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Hobi silinirken bir hata oluştu"));
        }
    }
}
