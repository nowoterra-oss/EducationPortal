using EduPortal.Application.DTOs.Coach;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoachesController : ControllerBase
{
    private readonly ICoachService _coachService;

    public CoachesController(ICoachService coachService)
    {
        _coachService = coachService;
    }

    /// <summary>
    /// Get all coaches
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CoachDto>>> GetAll()
    {
        var coaches = await _coachService.GetAllCoachesAsync();
        return Ok(coaches);
    }

    /// <summary>
    /// Get available coaches (for assignment)
    /// </summary>
    [HttpGet("available")]
    public async Task<ActionResult<IEnumerable<CoachSummaryDto>>> GetAvailable()
    {
        var coaches = await _coachService.GetAvailableCoachesAsync();
        return Ok(coaches);
    }

    /// <summary>
    /// Get coach by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CoachDto>> GetById(int id)
    {
        var coach = await _coachService.GetCoachByIdAsync(id);
        if (coach == null)
            return NotFound();

        return Ok(coach);
    }

    /// <summary>
    /// Get coaches by branch
    /// </summary>
    [HttpGet("branch/{branchId}")]
    public async Task<ActionResult<IEnumerable<CoachDto>>> GetByBranch(int branchId)
    {
        var coaches = await _coachService.GetCoachesByBranchAsync(branchId);
        return Ok(coaches);
    }

    /// <summary>
    /// Create new coach
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CoachDto>> Create([FromBody] CreateCoachDto dto)
    {
        var coach = await _coachService.CreateCoachAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = coach.Id }, coach);
    }

    /// <summary>
    /// Update coach
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Coach")]
    public async Task<ActionResult<CoachDto>> Update(int id, [FromBody] UpdateCoachDto dto)
    {
        try
        {
            var coach = await _coachService.UpdateCoachAsync(id, dto);
            return Ok(coach);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Delete coach (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _coachService.DeleteCoachAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Get coach performance metrics
    /// </summary>
    [HttpGet("{id}/performance")]
    public async Task<ActionResult<CoachPerformanceDto>> GetPerformance(
        int id,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = endDate ?? DateTime.UtcNow;

        try
        {
            var performance = await _coachService.GetCoachPerformanceAsync(id, start, end);
            return Ok(performance);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }
}
