using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Club;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Kulüp yönetimi endpoint'leri
/// </summary>
[ApiController]
[Route("api/clubs")]
[Produces("application/json")]
[Authorize]
public class ClubsController : ControllerBase
{
    private readonly IClubService _clubService;
    private readonly ILogger<ClubsController> _logger;

    public ClubsController(IClubService clubService, ILogger<ClubsController> logger)
    {
        _clubService = clubService;
        _logger = logger;
    }

    /// <summary>
    /// Tüm kulüpleri listele
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ClubDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ClubDto>>>> GetAll()
    {
        try
        {
            var clubs = await _clubService.GetAllAsync();
            return Ok(ApiResponse<List<ClubDto>>.SuccessResponse(clubs.ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving clubs");
            return StatusCode(500, ApiResponse<List<ClubDto>>.ErrorResponse("Kulüpler alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// ID ile kulüp detayı getir
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ClubDto>>> GetById(int id)
    {
        try
        {
            var club = await _clubService.GetByIdAsync(id);
            if (club == null)
                return NotFound(ApiResponse<ClubDto>.ErrorResponse("Kulüp bulunamadı"));

            return Ok(ApiResponse<ClubDto>.SuccessResponse(club));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving club {ClubId}", id);
            return StatusCode(500, ApiResponse<ClubDto>.ErrorResponse("Kulüp alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Kulüp üyelerini listele
    /// </summary>
    [HttpGet("{id}/members")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<ClubMemberDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<ClubMemberDto>>>> GetMembers(
        int id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var (items, totalCount) = await _clubService.GetMembersAsync(id, pageNumber, pageSize);
            var response = new PagedResponse<ClubMemberDto>(items.ToList(), pageNumber, pageSize, totalCount);
            return Ok(ApiResponse<PagedResponse<ClubMemberDto>>.SuccessResponse(response));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<PagedResponse<ClubMemberDto>>.ErrorResponse("Kulüp bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving club members {ClubId}", id);
            return StatusCode(500, ApiResponse<PagedResponse<ClubMemberDto>>.ErrorResponse("Kulüp üyeleri alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Öğrencinin kulüplerini getir
    /// </summary>
    [HttpGet("student/{studentId}")]
    [ProducesResponseType(typeof(ApiResponse<List<ClubDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ClubDto>>>> GetByStudent(int studentId)
    {
        try
        {
            var clubs = await _clubService.GetByStudentAsync(studentId);
            return Ok(ApiResponse<List<ClubDto>>.SuccessResponse(clubs.ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving clubs for student {StudentId}", studentId);
            return StatusCode(500, ApiResponse<List<ClubDto>>.ErrorResponse("Öğrenci kulüpleri alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Yeni kulüp oluştur (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ClubDto>>> Create([FromBody] CreateClubDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<ClubDto>.ErrorResponse("Geçersiz veri"));

            var club = await _clubService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = club.Id },
                ApiResponse<ClubDto>.SuccessResponse(club, "Kulüp başarıyla oluşturuldu"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating club");
            return StatusCode(500, ApiResponse<ClubDto>.ErrorResponse("Kulüp oluşturulurken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Kulübe katıl
    /// </summary>
    [HttpPost("{id}/join")]
    [ProducesResponseType(typeof(ApiResponse<ClubMemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ClubMemberDto>>> JoinClub(int id, [FromQuery] int studentId)
    {
        try
        {
            var member = await _clubService.JoinClubAsync(id, studentId);
            return Ok(ApiResponse<ClubMemberDto>.SuccessResponse(member, "Kulübe başarıyla katıldınız"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ClubMemberDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<ClubMemberDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining club {ClubId}", id);
            return StatusCode(500, ApiResponse<ClubMemberDto>.ErrorResponse("Kulübe katılırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Kulüpten ayrıl
    /// </summary>
    [HttpPost("{id}/leave")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> LeaveClub(int id, [FromQuery] int studentId)
    {
        try
        {
            var result = await _clubService.LeaveClubAsync(id, studentId);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Üyelik bulunamadı"));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Kulüpten başarıyla ayrıldınız"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving club {ClubId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Kulüpten ayrılırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Kulüp bilgilerini güncelle (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<ClubDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ClubDto>>> Update(int id, [FromBody] UpdateClubDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<ClubDto>.ErrorResponse("Geçersiz veri"));

            var club = await _clubService.UpdateAsync(id, dto);
            return Ok(ApiResponse<ClubDto>.SuccessResponse(club, "Kulüp başarıyla güncellendi"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<ClubDto>.ErrorResponse("Kulüp bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating club {ClubId}", id);
            return StatusCode(500, ApiResponse<ClubDto>.ErrorResponse("Kulüp güncellenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Kulübü sil (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _clubService.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Kulüp bulunamadı"));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Kulüp başarıyla silindi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting club {ClubId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Kulüp silinirken bir hata oluştu"));
        }
    }
}
