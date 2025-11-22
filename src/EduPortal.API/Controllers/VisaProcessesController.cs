using EduPortal.Application.DTOs.Visa;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/visa-processes")]
[Authorize]
public class VisaProcessesController : ControllerBase
{
    private readonly IVisaProcessService _service;

    public VisaProcessesController(IVisaProcessService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Coach,Danışman")]
    public async Task<ActionResult<IEnumerable<VisaProcessDto>>> GetAll()
    {
        var visaProcesses = await _service.GetAllVisaProcessesAsync();
        return Ok(visaProcesses);
    }

    [HttpGet("active")]
    [Authorize(Roles = "Admin,Coach,Danışman")]
    public async Task<ActionResult<IEnumerable<VisaProcessDto>>> GetActive()
    {
        var visaProcesses = await _service.GetActiveVisaProcessesAsync();
        return Ok(visaProcesses);
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin,Coach,Danışman")]
    public async Task<ActionResult<IEnumerable<VisaProcessDto>>> GetPending()
    {
        var visaProcesses = await _service.GetPendingVisaProcessesAsync();
        return Ok(visaProcesses);
    }

    [HttpGet("expiring")]
    [Authorize(Roles = "Admin,Coach,Danışman")]
    public async Task<ActionResult<IEnumerable<VisaProcessDto>>> GetExpiring([FromQuery] int days = 90)
    {
        var visaProcesses = await _service.GetExpiringVisasAsync(days);
        return Ok(visaProcesses);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Coach,Danışman,Ogrenci")]
    public async Task<ActionResult<VisaProcessDto>> GetById(int id)
    {
        var visaProcess = await _service.GetVisaProcessByIdAsync(id);
        if (visaProcess == null)
            return NotFound($"Visa process with ID {id} not found");

        return Ok(visaProcess);
    }

    [HttpGet("program/{programId}")]
    [Authorize(Roles = "Admin,Coach,Danışman,Ogrenci")]
    public async Task<ActionResult<IEnumerable<VisaProcessDto>>> GetByProgram(int programId)
    {
        var visaProcesses = await _service.GetVisaProcessesByProgramAsync(programId);
        return Ok(visaProcesses);
    }

    [HttpGet("{id}/timeline")]
    [Authorize(Roles = "Admin,Coach,Danışman,Ogrenci")]
    public async Task<ActionResult<VisaTimelineDto>> GetTimeline(int id)
    {
        try
        {
            var timeline = await _service.GetVisaTimelineAsync(id);
            return Ok(timeline);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,Coach,Danışman")]
    public async Task<ActionResult<VisaStatisticsDto>> GetStatistics()
    {
        var stats = await _service.GetStatisticsAsync();
        return Ok(stats);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Coach,Danışman")]
    public async Task<ActionResult<VisaProcessDto>> Create([FromBody] CreateVisaProcessDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var visaProcess = await _service.CreateVisaProcessAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = visaProcess.Id }, visaProcess);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Coach,Danışman")]
    public async Task<ActionResult<VisaProcessDto>> Update(int id, [FromBody] UpdateVisaProcessDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var visaProcess = await _service.UpdateVisaProcessAsync(id, dto);
            return Ok(visaProcess);
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
        var result = await _service.DeleteVisaProcessAsync(id);
        if (!result)
            return NotFound($"Visa process with ID {id} not found");

        return NoContent();
    }
}
