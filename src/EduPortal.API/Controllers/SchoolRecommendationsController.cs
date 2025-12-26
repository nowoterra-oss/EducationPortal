using EduPortal.Application.DTOs.SchoolRecommendation;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/school-recommendations")]
[Authorize]
public class SchoolRecommendationsController : ControllerBase
{
    private readonly ISchoolRecommendationService _recommendationService;

    public SchoolRecommendationsController(ISchoolRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    /// <summary>
    /// Get all school recommendations
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SchoolRecommendationDto>>> GetAll()
    {
        var recommendations = await _recommendationService.GetAllRecommendationsAsync();
        return Ok(recommendations);
    }

    /// <summary>
    /// Get school recommendation summaries
    /// </summary>
    [HttpGet("summaries")]
    public async Task<ActionResult<IEnumerable<SchoolRecommendationSummaryDto>>> GetSummaries()
    {
        var summaries = await _recommendationService.GetRecommendationSummariesAsync();
        return Ok(summaries);
    }

    /// <summary>
    /// Get school recommendation by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<SchoolRecommendationDto>> GetById(int id)
    {
        var recommendation = await _recommendationService.GetRecommendationByIdAsync(id);
        if (recommendation == null)
            return NotFound();

        return Ok(recommendation);
    }

    /// <summary>
    /// Get school recommendations by student
    /// </summary>
    [HttpGet("student/{studentId}")]
    public async Task<ActionResult<IEnumerable<SchoolRecommendationDto>>> GetByStudent(int studentId)
    {
        var recommendations = await _recommendationService.GetRecommendationsByStudentAsync(studentId);
        return Ok(recommendations);
    }

    /// <summary>
    /// Get school recommendations by counselor
    /// </summary>
    [HttpGet("counselor/{counselorId}")]
    public async Task<ActionResult<IEnumerable<SchoolRecommendationDto>>> GetByCounselor(int counselorId)
    {
        var recommendations = await _recommendationService.GetRecommendationsByCounselorAsync(counselorId);
        return Ok(recommendations);
    }

    /// <summary>
    /// Get school recommendation statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<SchoolRecommendationStatisticsDto>> GetStatistics()
    {
        var statistics = await _recommendationService.GetStatisticsAsync();
        return Ok(statistics);
    }

    /// <summary>
    /// Create new school recommendation
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Danışman")]
    public async Task<ActionResult<SchoolRecommendationDto>> Create([FromBody] CreateSchoolRecommendationDto dto)
    {
        try
        {
            var recommendation = await _recommendationService.CreateRecommendationAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = recommendation.Id }, recommendation);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update school recommendation
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Danışman")]
    public async Task<ActionResult<SchoolRecommendationDto>> Update(int id, [FromBody] UpdateSchoolRecommendationDto dto)
    {
        try
        {
            var recommendation = await _recommendationService.UpdateRecommendationAsync(id, dto);
            return Ok(recommendation);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Delete school recommendation (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _recommendationService.DeleteRecommendationAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
