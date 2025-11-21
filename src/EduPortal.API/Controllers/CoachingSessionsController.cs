using EduPortal.Application.DTOs.CoachingSession;
using EduPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/coaching-sessions")]
[Authorize]
public class CoachingSessionsController : ControllerBase
{
    private readonly ICoachingSessionService _sessionService;

    public CoachingSessionsController(ICoachingSessionService sessionService)
    {
        _sessionService = sessionService;
    }

    /// <summary>
    /// Get all coaching sessions
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CoachingSessionDto>>> GetAll()
    {
        var sessions = await _sessionService.GetAllSessionsAsync();
        return Ok(sessions);
    }

    /// <summary>
    /// Get upcoming sessions
    /// </summary>
    [HttpGet("upcoming")]
    public async Task<ActionResult<IEnumerable<CoachingSessionDto>>> GetUpcoming([FromQuery] int days = 7)
    {
        var sessions = await _sessionService.GetUpcomingSessionsAsync(days);
        return Ok(sessions);
    }

    /// <summary>
    /// Get sessions by student
    /// </summary>
    [HttpGet("student/{studentId}")]
    public async Task<ActionResult<IEnumerable<CoachingSessionDto>>> GetByStudent(int studentId)
    {
        var sessions = await _sessionService.GetSessionsByStudentAsync(studentId);
        return Ok(sessions);
    }

    /// <summary>
    /// Get sessions by coach
    /// </summary>
    [HttpGet("coach/{coachId}")]
    public async Task<ActionResult<IEnumerable<CoachingSessionDto>>> GetByCoach(int coachId)
    {
        var sessions = await _sessionService.GetSessionsByCoachAsync(coachId);
        return Ok(sessions);
    }

    /// <summary>
    /// Get sessions by date range
    /// </summary>
    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<CoachingSessionDto>>> GetByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var sessions = await _sessionService.GetSessionsByDateRangeAsync(startDate, endDate);
        return Ok(sessions);
    }

    /// <summary>
    /// Get calendar events (for calendar view)
    /// </summary>
    [HttpGet("calendar")]
    public async Task<ActionResult<IEnumerable<SessionCalendarDto>>> GetCalendarEvents(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var events = await _sessionService.GetCalendarEventsAsync(startDate, endDate);
        return Ok(events);
    }

    /// <summary>
    /// Get session by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CoachingSessionDto>> GetById(int id)
    {
        var session = await _sessionService.GetSessionByIdAsync(id);
        if (session == null)
            return NotFound();

        return Ok(session);
    }

    /// <summary>
    /// Create new session
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Coach,Danışman")]
    public async Task<ActionResult<CoachingSessionDto>> Create([FromBody] CreateCoachingSessionDto dto)
    {
        try
        {
            var session = await _sessionService.CreateSessionAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = session.Id }, session);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update session
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Coach,Danışman")]
    public async Task<ActionResult<CoachingSessionDto>> Update(int id, [FromBody] UpdateCoachingSessionDto dto)
    {
        try
        {
            var session = await _sessionService.UpdateSessionAsync(id, dto);
            return Ok(session);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Complete session
    /// </summary>
    [HttpPost("{id}/complete")]
    [Authorize(Roles = "Admin,Coach,Danışman")]
    public async Task<ActionResult<CoachingSessionDto>> Complete(int id, [FromBody] CompleteSessionDto dto)
    {
        try
        {
            var session = await _sessionService.CompleteSessionAsync(id, dto);
            return Ok(session);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Cancel session
    /// </summary>
    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Admin,Coach,Danışman")]
    public async Task<ActionResult> Cancel(int id, [FromBody] string reason)
    {
        var result = await _sessionService.CancelSessionAsync(id, reason);
        if (!result)
            return NotFound();

        return Ok(new { message = "Session cancelled successfully" });
    }

    /// <summary>
    /// Delete session (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _sessionService.DeleteSessionAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
