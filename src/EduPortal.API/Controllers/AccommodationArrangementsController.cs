using EduPortal.Application.DTOs.Accommodation;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/accommodation-arrangements")]
[Authorize]
public class AccommodationArrangementsController : ControllerBase
{
    private readonly IAccommodationArrangementService _service;

    public AccommodationArrangementsController(IAccommodationArrangementService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Coach,Danışman")]
    public async Task<ActionResult<IEnumerable<AccommodationArrangementDto>>> GetAll()
    {
        var arrangements = await _service.GetAllArrangementsAsync();
        return Ok(arrangements);
    }

    [HttpGet("active")]
    [Authorize(Roles = "Admin,Coach,Danışman")]
    public async Task<ActionResult<IEnumerable<AccommodationArrangementDto>>> GetActive()
    {
        var arrangements = await _service.GetActiveArrangementsAsync();
        return Ok(arrangements);
    }

    [HttpGet("summaries")]
    [Authorize(Roles = "Admin,Coach,Danışman")]
    public async Task<ActionResult<IEnumerable<AccommodationSummaryDto>>> GetSummaries()
    {
        var summaries = await _service.GetArrangementSummariesAsync();
        return Ok(summaries);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Coach,Danışman,Ogrenci")]
    public async Task<ActionResult<AccommodationArrangementDto>> GetById(int id)
    {
        var arrangement = await _service.GetArrangementByIdAsync(id);
        if (arrangement == null)
            return NotFound($"Accommodation arrangement with ID {id} not found");

        return Ok(arrangement);
    }

    [HttpGet("program/{programId}")]
    [Authorize(Roles = "Admin,Coach,Danışman,Ogrenci")]
    public async Task<ActionResult<IEnumerable<AccommodationArrangementDto>>> GetByProgram(int programId)
    {
        var arrangements = await _service.GetArrangementsByProgramAsync(programId);
        return Ok(arrangements);
    }

    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,Coach,Danışman")]
    public async Task<ActionResult<AccommodationStatisticsDto>> GetStatistics()
    {
        var stats = await _service.GetStatisticsAsync();
        return Ok(stats);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Coach,Danışman")]
    public async Task<ActionResult<AccommodationArrangementDto>> Create([FromBody] CreateAccommodationArrangementDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var arrangement = await _service.CreateArrangementAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = arrangement.Id }, arrangement);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Coach,Danışman")]
    public async Task<ActionResult<AccommodationArrangementDto>> Update(int id, [FromBody] UpdateAccommodationArrangementDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var arrangement = await _service.UpdateArrangementAsync(id, dto);
            return Ok(arrangement);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _service.DeleteArrangementAsync(id);
        if (!result)
            return NotFound($"Accommodation arrangement with ID {id} not found");

        return NoContent();
    }
}
