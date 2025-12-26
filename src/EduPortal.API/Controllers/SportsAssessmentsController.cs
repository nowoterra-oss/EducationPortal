using EduPortal.Application.DTOs.SportsAssessment;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/sports-assessments")]
[Authorize]
public class SportsAssessmentsController : ControllerBase
{
    private readonly ISportsAssessmentService _assessmentService;

    public SportsAssessmentsController(ISportsAssessmentService assessmentService)
    {
        _assessmentService = assessmentService;
    }

    /// <summary>
    /// Get all sports assessments
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SportsAssessmentDto>>> GetAll()
    {
        var assessments = await _assessmentService.GetAllAssessmentsAsync();
        return Ok(assessments);
    }

    /// <summary>
    /// Get sports assessment summaries
    /// </summary>
    [HttpGet("summaries")]
    public async Task<ActionResult<IEnumerable<SportsAssessmentSummaryDto>>> GetSummaries()
    {
        var summaries = await _assessmentService.GetAssessmentSummariesAsync();
        return Ok(summaries);
    }

    /// <summary>
    /// Get sports assessment by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<SportsAssessmentDto>> GetById(int id)
    {
        var assessment = await _assessmentService.GetAssessmentByIdAsync(id);
        if (assessment == null)
            return NotFound();

        return Ok(assessment);
    }

    /// <summary>
    /// Get sports assessments by student
    /// </summary>
    [HttpGet("student/{studentId}")]
    public async Task<ActionResult<IEnumerable<SportsAssessmentDto>>> GetByStudent(int studentId)
    {
        var assessments = await _assessmentService.GetAssessmentsByStudentAsync(studentId);
        return Ok(assessments);
    }

    /// <summary>
    /// Get sports assessments by counselor
    /// </summary>
    [HttpGet("counselor/{counselorId}")]
    public async Task<ActionResult<IEnumerable<SportsAssessmentDto>>> GetByCounselor(int counselorId)
    {
        var assessments = await _assessmentService.GetAssessmentsByCounselorAsync(counselorId);
        return Ok(assessments);
    }

    /// <summary>
    /// Get sports assessment statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<SportsAssessmentStatisticsDto>> GetStatistics()
    {
        var statistics = await _assessmentService.GetStatisticsAsync();
        return Ok(statistics);
    }

    /// <summary>
    /// Create new sports assessment
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Danışman")]
    public async Task<ActionResult<SportsAssessmentDto>> Create([FromBody] CreateSportsAssessmentDto dto)
    {
        try
        {
            var assessment = await _assessmentService.CreateAssessmentAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = assessment.Id }, assessment);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update sports assessment
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Danışman")]
    public async Task<ActionResult<SportsAssessmentDto>> Update(int id, [FromBody] UpdateSportsAssessmentDto dto)
    {
        try
        {
            var assessment = await _assessmentService.UpdateAssessmentAsync(id, dto);
            return Ok(assessment);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Delete sports assessment (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _assessmentService.DeleteAssessmentAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
