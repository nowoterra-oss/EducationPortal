using EduPortal.Application.DTOs.Branch;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BranchesController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchesController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    /// <summary>
    /// Get all branches
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BranchDto>>> GetAll()
    {
        var branches = await _branchService.GetAllBranchesAsync();
        return Ok(branches);
    }

    /// <summary>
    /// Get branch by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<BranchDto>> GetById(int id)
    {
        var branch = await _branchService.GetBranchByIdAsync(id);
        if (branch == null)
            return NotFound();

        return Ok(branch);
    }

    /// <summary>
    /// Create new branch
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BranchDto>> Create([FromBody] CreateBranchDto dto)
    {
        var branch = await _branchService.CreateBranchAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = branch.Id }, branch);
    }

    /// <summary>
    /// Update branch
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,BranchManager")]
    public async Task<ActionResult<BranchDto>> Update(int id, [FromBody] UpdateBranchDto dto)
    {
        try
        {
            var branch = await _branchService.UpdateBranchAsync(id, dto);
            return Ok(branch);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Delete branch (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _branchService.DeleteBranchAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Get branch statistics
    /// </summary>
    [HttpGet("{id}/statistics")]
    public async Task<ActionResult<BranchStatisticsDto>> GetStatistics(int id)
    {
        try
        {
            var stats = await _branchService.GetBranchStatisticsAsync(id);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Get performance comparison for all branches
    /// </summary>
    [HttpGet("performance-comparison")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<BranchStatisticsDto>>> GetPerformanceComparison()
    {
        var stats = await _branchService.GetAllBranchesPerformanceAsync();
        return Ok(stats);
    }

    /// <summary>
    /// Transfer student to another branch
    /// </summary>
    [HttpPost("transfer-student")]
    [Authorize(Roles = "Admin,BranchManager")]
    public async Task<ActionResult> TransferStudent([FromBody] TransferStudentDto dto)
    {
        var result = await _branchService.TransferStudentAsync(dto);
        if (!result)
            return BadRequest("Transfer failed");

        return Ok(new { message = "Student transferred successfully" });
    }
}
