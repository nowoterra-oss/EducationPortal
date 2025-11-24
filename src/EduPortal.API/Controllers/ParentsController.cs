using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Parent;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Parent management endpoints
/// </summary>
[ApiController]
[Route("api/parents")]
[Produces("application/json")]
[Authorize]
public class ParentsController : ControllerBase
{
    private readonly IParentService _parentService;
    private readonly ILogger<ParentsController> _logger;

    public ParentsController(IParentService parentService, ILogger<ParentsController> logger)
    {
        _parentService = parentService;
        _logger = logger;
    }

    /// <summary>
    /// Get all parents with pagination
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Danışman")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<ParentSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<ParentSummaryDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var (items, totalCount) = await _parentService.GetParentsPagedAsync(pageNumber, pageSize);
            var pagedResponse = new PagedResponse<ParentSummaryDto>(
                items.ToList(),
                totalCount,
                pageNumber,
                pageSize);

            return Ok(ApiResponse<PagedResponse<ParentSummaryDto>>.SuccessResponse(pagedResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting parents");
            return StatusCode(500, ApiResponse<PagedResponse<ParentSummaryDto>>.ErrorResponse("Veliler alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get parent by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ParentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ParentDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ParentDto>>> GetById(int id)
    {
        try
        {
            var parent = await _parentService.GetParentByIdAsync(id);
            if (parent == null)
            {
                return NotFound(ApiResponse<ParentDto>.ErrorResponse("Veli bulunamadı"));
            }

            return Ok(ApiResponse<ParentDto>.SuccessResponse(parent));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting parent {ParentId}", id);
            return StatusCode(500, ApiResponse<ParentDto>.ErrorResponse("Veli alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get parents by student ID
    /// </summary>
    [HttpGet("by-student/{studentId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ParentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ParentDto>>>> GetByStudentId(int studentId)
    {
        try
        {
            var parents = await _parentService.GetParentsByStudentIdAsync(studentId);
            return Ok(ApiResponse<IEnumerable<ParentDto>>.SuccessResponse(parents));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting parents for student {StudentId}", studentId);
            return StatusCode(500, ApiResponse<IEnumerable<ParentDto>>.ErrorResponse("Veliler alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Create new parent
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Kayıtçı")]
    [ProducesResponseType(typeof(ApiResponse<ParentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ParentDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ParentDto>>> Create([FromBody] CreateParentDto parentDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ParentDto>.ErrorResponse("Geçersiz veri"));
            }

            var parent = await _parentService.CreateParentAsync(parentDto);
            return CreatedAtAction(nameof(GetById), new { id = parent.Id },
                ApiResponse<ParentDto>.SuccessResponse(parent, "Veli başarıyla oluşturuldu"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating parent");
            return StatusCode(500, ApiResponse<ParentDto>.ErrorResponse("Veli oluşturulurken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Update parent
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<ParentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ParentDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ParentDto>>> Update(int id, [FromBody] UpdateParentDto parentDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ParentDto>.ErrorResponse("Geçersiz veri"));
            }

            var parent = await _parentService.UpdateParentAsync(id, parentDto);
            return Ok(ApiResponse<ParentDto>.SuccessResponse(parent, "Veli başarıyla güncellendi"));
        }
        catch (Exception ex) when (ex.Message == "Parent not found")
        {
            return NotFound(ApiResponse<ParentDto>.ErrorResponse("Veli bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating parent {ParentId}", id);
            return StatusCode(500, ApiResponse<ParentDto>.ErrorResponse("Veli güncellenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Delete parent
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _parentService.DeleteParentAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Veli bulunamadı"));
            }

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Veli başarıyla silindi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting parent {ParentId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Veli silinirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Add student relationship to parent
    /// </summary>
    [HttpPost("{parentId}/students")]
    [Authorize(Roles = "Admin,Kayıtçı")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> AddStudentRelationship(
        int parentId,
        [FromBody] StudentRelationshipDto relationship)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse("Geçersiz veri"));
            }

            var result = await _parentService.AddStudentRelationshipAsync(parentId, relationship);
            if (!result)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse("İlişki eklenemedi. Veli bulunamadı veya ilişki zaten mevcut."));
            }

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Öğrenci ilişkisi başarıyla eklendi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding student relationship for parent {ParentId}", parentId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("İlişki eklenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Remove student relationship from parent
    /// </summary>
    [HttpDelete("{parentId}/students/{studentId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveStudentRelationship(int parentId, int studentId)
    {
        try
        {
            var result = await _parentService.RemoveStudentRelationshipAsync(parentId, studentId);
            if (!result)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("İlişki bulunamadı"));
            }

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Öğrenci ilişkisi başarıyla kaldırıldı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing student relationship for parent {ParentId}", parentId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("İlişki kaldırılırken bir hata oluştu"));
        }
    }
}
