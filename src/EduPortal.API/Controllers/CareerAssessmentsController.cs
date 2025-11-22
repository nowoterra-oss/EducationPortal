using EduPortal.Application.DTOs.Assessment;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/career-assessments")]
[Authorize]
public class CareerAssessmentsController : ControllerBase
{
    private readonly ICareerAssessmentService _assessmentService;

    public CareerAssessmentsController(ICareerAssessmentService assessmentService)
    {
        _assessmentService = assessmentService;
    }

    /// <summary>
    /// Get all career assessments
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CareerAssessmentDto>>> GetAll()
    {
        var assessments = await _assessmentService.GetAllAssessmentsAsync();
        return Ok(assessments);
    }

    /// <summary>
    /// Get career assessment summaries
    /// </summary>
    [HttpGet("summaries")]
    public async Task<ActionResult<IEnumerable<CareerAssessmentSummaryDto>>> GetSummaries()
    {
        var summaries = await _assessmentService.GetAssessmentSummariesAsync();
        return Ok(summaries);
    }

    /// <summary>
    /// Get career assessment by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CareerAssessmentDto>> GetById(int id)
    {
        var assessment = await _assessmentService.GetAssessmentByIdAsync(id);
        if (assessment == null)
            return NotFound();

        return Ok(assessment);
    }

    /// <summary>
    /// Get career assessments by student
    /// </summary>
    [HttpGet("student/{studentId}")]
    public async Task<ActionResult<IEnumerable<CareerAssessmentDto>>> GetByStudent(int studentId)
    {
        var assessments = await _assessmentService.GetAssessmentsByStudentAsync(studentId);
        return Ok(assessments);
    }

    /// <summary>
    /// Get career assessments by coach
    /// </summary>
    [HttpGet("coach/{coachId}")]
    public async Task<ActionResult<IEnumerable<CareerAssessmentDto>>> GetByCoach(int coachId)
    {
        var assessments = await _assessmentService.GetAssessmentsByCoachAsync(coachId);
        return Ok(assessments);
    }

    /// <summary>
    /// Get career assessment statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<CareerAssessmentStatisticsDto>> GetStatistics()
    {
        var statistics = await _assessmentService.GetStatisticsAsync();
        return Ok(statistics);
    }

    /// <summary>
    /// Create new career assessment
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Coach,Danışman")]
    public async Task<ActionResult<CareerAssessmentDto>> Create([FromBody] CreateCareerAssessmentDto dto)
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
    /// Update career assessment
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Coach,Danışman")]
    public async Task<ActionResult<CareerAssessmentDto>> Update(int id, [FromBody] UpdateCareerAssessmentDto dto)
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
    /// Delete career assessment (soft delete)
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
