using EduPortal.API.Attributes;
using EduPortal.Application.Common;
using EduPortal.Application.DTOs.AGP;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Academic Development Plan (AGP - Akademik Gelişim Planı) endpoints
/// </summary>
[ApiController]
[Route("api/agp")]
[Produces("application/json")]
[Authorize]
public class AGPController : ControllerBase
{
    private readonly IAGPService _agpService;
    private readonly ILogger<AGPController> _logger;

    public AGPController(IAGPService agpService, ILogger<AGPController> logger)
    {
        _agpService = agpService;
        _logger = logger;
    }

    /// <summary>
    /// Get all AGP records with pagination
    /// </summary>
    [HttpGet]
    [RequirePermission(Permissions.AgpView)]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<AGPDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<AGPDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var (items, totalCount) = await _agpService.GetAllPagedAsync(pageNumber, pageSize);

            var pagedResponse = new PagedResponse<AGPDto>(
                items.ToList(),
                totalCount,
                pageNumber,
                pageSize);

            return Ok(ApiResponse<PagedResponse<AGPDto>>.SuccessResponse(pagedResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AGP kayıtları getirilirken hata oluştu");
            return StatusCode(500, ApiResponse<PagedResponse<AGPDto>>.ErrorResponse("AGP kayıtları getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get AGP by ID
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission(Permissions.AgpView)]
    [ProducesResponseType(typeof(ApiResponse<AGPDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AGPDto>>> GetById(int id)
    {
        try
        {
            var agp = await _agpService.GetByIdAsync(id);

            if (agp == null)
                return NotFound(ApiResponse<AGPDto>.ErrorResponse("AGP bulunamadı"));

            return Ok(ApiResponse<AGPDto>.SuccessResponse(agp));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AGP getirilirken hata oluştu. ID: {AGPId}", id);
            return StatusCode(500, ApiResponse<AGPDto>.ErrorResponse("AGP getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Create new AGP
    /// </summary>
    [HttpPost]
    [RequirePermission(Permissions.AgpCreate)]
    [ProducesResponseType(typeof(ApiResponse<AGPDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AGPDto>>> Create([FromBody] CreateAGPDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<AGPDto>.ErrorResponse("Geçersiz veri"));

            var agp = await _agpService.CreateAsync(createDto);

            return CreatedAtAction(nameof(GetById), new { id = agp.Id },
                ApiResponse<AGPDto>.SuccessResponse(agp, "AGP başarıyla oluşturuldu"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AGP oluşturulurken hata oluştu");
            return StatusCode(500, ApiResponse<AGPDto>.ErrorResponse("AGP oluşturulurken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Update AGP
    /// </summary>
    [HttpPut("{id}")]
    [RequirePermission(Permissions.AgpEdit)]
    [ProducesResponseType(typeof(ApiResponse<AGPDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AGPDto>>> Update(int id, [FromBody] UpdateAGPDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<AGPDto>.ErrorResponse("Geçersiz veri"));

            var agp = await _agpService.UpdateAsync(id, updateDto);

            return Ok(ApiResponse<AGPDto>.SuccessResponse(agp, "AGP başarıyla güncellendi"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<AGPDto>.ErrorResponse("AGP bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AGP güncellenirken hata oluştu. ID: {AGPId}", id);
            return StatusCode(500, ApiResponse<AGPDto>.ErrorResponse("AGP güncellenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Delete AGP
    /// </summary>
    [HttpDelete("{id}")]
    [RequirePermission(Permissions.AgpEdit)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var result = await _agpService.DeleteAsync(id);

            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("AGP bulunamadı"));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "AGP başarıyla silindi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AGP silinirken hata oluştu. ID: {AGPId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("AGP silinirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get student AGP
    /// </summary>
    [HttpGet("student/{studentId}")]
    [RequirePermission(Permissions.AgpView)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AGPDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<AGPDto>>>> GetByStudent(int studentId)
    {
        try
        {
            var agps = await _agpService.GetByStudentAsync(studentId);
            return Ok(ApiResponse<IEnumerable<AGPDto>>.SuccessResponse(agps));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Öğrenci AGP kayıtları getirilirken hata oluştu. StudentId: {StudentId}", studentId);
            return StatusCode(500, ApiResponse<IEnumerable<AGPDto>>.ErrorResponse("AGP kayıtları getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get AGP goals (milestones)
    /// </summary>
    [HttpGet("{id}/goals")]
    [RequirePermission(Permissions.AgpView)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AGPGoalDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<AGPGoalDto>>>> GetGoals(int id)
    {
        try
        {
            var goals = await _agpService.GetGoalsAsync(id);
            return Ok(ApiResponse<IEnumerable<AGPGoalDto>>.SuccessResponse(goals));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AGP hedefleri getirilirken hata oluştu. AGPId: {AGPId}", id);
            return StatusCode(500, ApiResponse<IEnumerable<AGPGoalDto>>.ErrorResponse("Hedefler getirilirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Add goal to AGP
    /// </summary>
    [HttpPost("{id}/goals")]
    [RequirePermission(Permissions.AgpCreate)]
    [ProducesResponseType(typeof(ApiResponse<AGPGoalDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AGPGoalDto>>> AddGoal(int id, [FromBody] CreateAGPGoalDto goalDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<AGPGoalDto>.ErrorResponse("Geçersiz veri"));

            var goal = await _agpService.AddGoalAsync(id, goalDto);

            return CreatedAtAction(nameof(GetGoals), new { id },
                ApiResponse<AGPGoalDto>.SuccessResponse(goal, "Hedef başarıyla eklendi"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<AGPGoalDto>.ErrorResponse("AGP bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hedef eklenirken hata oluştu. AGPId: {AGPId}", id);
            return StatusCode(500, ApiResponse<AGPGoalDto>.ErrorResponse("Hedef eklenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Update goal
    /// </summary>
    [HttpPut("{id}/goals/{goalId}")]
    [RequirePermission(Permissions.AgpEdit)]
    [ProducesResponseType(typeof(ApiResponse<AGPGoalDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AGPGoalDto>>> UpdateGoal(int id, int goalId, [FromBody] UpdateAGPGoalDto goalDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<AGPGoalDto>.ErrorResponse("Geçersiz veri"));

            var goal = await _agpService.UpdateGoalAsync(id, goalId, goalDto);

            return Ok(ApiResponse<AGPGoalDto>.SuccessResponse(goal, "Hedef başarıyla güncellendi"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<AGPGoalDto>.ErrorResponse("Hedef bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hedef güncellenirken hata oluştu. AGPId: {AGPId}, GoalId: {GoalId}", id, goalId);
            return StatusCode(500, ApiResponse<AGPGoalDto>.ErrorResponse("Hedef güncellenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Delete goal
    /// </summary>
    [HttpDelete("{id}/goals/{goalId}")]
    [RequirePermission(Permissions.AgpEdit)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteGoal(int id, int goalId)
    {
        try
        {
            var result = await _agpService.DeleteGoalAsync(id, goalId);

            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Hedef bulunamadı"));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Hedef başarıyla silindi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hedef silinirken hata oluştu. AGPId: {AGPId}, GoalId: {GoalId}", id, goalId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Hedef silinirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get AGP progress
    /// </summary>
    [HttpGet("{id}/progress")]
    [RequirePermission(Permissions.AgpView)]
    [ProducesResponseType(typeof(ApiResponse<AGPProgressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AGPProgressDto>>> GetProgress(int id)
    {
        try
        {
            var progress = await _agpService.GetProgressAsync(id);
            return Ok(ApiResponse<AGPProgressDto>.SuccessResponse(progress));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<AGPProgressDto>.ErrorResponse("AGP bulunamadı"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AGP ilerleme durumu getirilirken hata oluştu. AGPId: {AGPId}", id);
            return StatusCode(500, ApiResponse<AGPProgressDto>.ErrorResponse("İlerleme durumu getirilirken bir hata oluştu"));
        }
    }
}
