using EduPortal.Application.DTOs.StudyAbroad;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/study-abroad")]
[Authorize]
public class StudyAbroadController : ControllerBase
{
    private readonly IStudyAbroadService _studyAbroadService;

    public StudyAbroadController(IStudyAbroadService studyAbroadService)
    {
        _studyAbroadService = studyAbroadService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Danışman")]
    public async Task<ActionResult<IEnumerable<StudyAbroadProgramDto>>> GetAllPrograms()
    {
        var programs = await _studyAbroadService.GetAllProgramsAsync();
        return Ok(programs);
    }

    [HttpGet("active")]
    [Authorize(Roles = "Admin,Danışman")]
    public async Task<ActionResult<IEnumerable<StudyAbroadProgramDto>>> GetActivePrograms()
    {
        var programs = await _studyAbroadService.GetActiveProgramsAsync();
        return Ok(programs);
    }

    [HttpGet("summaries")]
    [Authorize(Roles = "Admin,Danışman")]
    public async Task<ActionResult<IEnumerable<ProgramSummaryDto>>> GetProgramSummaries()
    {
        var summaries = await _studyAbroadService.GetProgramSummariesAsync();
        return Ok(summaries);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Danışman,Ogrenci")]
    public async Task<ActionResult<StudyAbroadProgramDto>> GetProgramById(int id)
    {
        var program = await _studyAbroadService.GetProgramByIdAsync(id);
        if (program == null)
            return NotFound($"Study abroad program with ID {id} not found");

        return Ok(program);
    }

    [HttpGet("student/{studentId}")]
    [Authorize(Roles = "Admin,Danışman,Ogrenci")]
    public async Task<ActionResult<IEnumerable<StudyAbroadProgramDto>>> GetProgramsByStudent(int studentId)
    {
        var programs = await _studyAbroadService.GetProgramsByStudentAsync(studentId);
        return Ok(programs);
    }

    [HttpGet("counselor/{counselorId}")]
    [Authorize(Roles = "Admin,Danışman")]
    public async Task<ActionResult<IEnumerable<StudyAbroadProgramDto>>> GetProgramsByCounselor(int counselorId)
    {
        var programs = await _studyAbroadService.GetProgramsByCounselorAsync(counselorId);
        return Ok(programs);
    }

    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,Danışman")]
    public async Task<ActionResult<ProgramStatisticsDto>> GetStatistics()
    {
        var statistics = await _studyAbroadService.GetStatisticsAsync();
        return Ok(statistics);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Danışman")]
    public async Task<ActionResult<StudyAbroadProgramDto>> CreateProgram([FromBody] CreateStudyAbroadProgramDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var program = await _studyAbroadService.CreateProgramAsync(dto);
        return CreatedAtAction(nameof(GetProgramById), new { id = program.Id }, program);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Danışman")]
    public async Task<ActionResult<StudyAbroadProgramDto>> UpdateProgram(int id, [FromBody] UpdateStudyAbroadProgramDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var program = await _studyAbroadService.UpdateProgramAsync(id, dto);
            return Ok(program);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteProgram(int id)
    {
        var success = await _studyAbroadService.DeleteProgramAsync(id);
        if (!success)
            return NotFound($"Study abroad program with ID {id} not found");

        return NoContent();
    }
}
