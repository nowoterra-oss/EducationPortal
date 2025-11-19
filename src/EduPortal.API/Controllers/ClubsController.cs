using EduPortal.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Student club management endpoints
/// </summary>
[ApiController]
[Route("api/clubs")]
[Produces("application/json")]
[Authorize]
public class ClubsController : ControllerBase
{
    // TODO: Implement IClubService
    private readonly ILogger<ClubsController> _logger;

    public ClubsController(ILogger<ClubsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get all clubs
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<object>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetAll()
    {
        // TODO: Implement service
        return Ok(ApiResponse<List<object>>.ErrorResponse("Servis henüz implement edilmedi"));
    }

    /// <summary>
    /// Get club by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> GetById(int id)
    {
        // TODO: Implement service
        return Ok(ApiResponse<object>.ErrorResponse("Servis henüz implement edilmedi"));
    }

    /// <summary>
    /// Create club
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] object clubDto)
    {
        // TODO: Implement service
        return Ok(ApiResponse<object>.ErrorResponse("Servis henüz implement edilmedi"));
    }

    /// <summary>
    /// Delete club
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        // TODO: Implement service
        return Ok(ApiResponse<bool>.ErrorResponse("Servis henüz implement edilmedi"));
    }
}
