using EduPortal.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Student hobby management endpoints
/// </summary>
[ApiController]
[Route("api/hobbies")]
[Produces("application/json")]
[Authorize]
public class HobbiesController : ControllerBase
{
    // TODO: Implement IHobbyService
    private readonly ILogger<HobbiesController> _logger;

    public HobbiesController(ILogger<HobbiesController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get all hobbies
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<object>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetAll()
    {
        // TODO: Implement service
        return Ok(ApiResponse<List<object>>.ErrorResponse("Servis henüz implement edilmedi"));
    }

    /// <summary>
    /// Get hobby by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> GetById(int id)
    {
        // TODO: Implement service
        return Ok(ApiResponse<object>.ErrorResponse("Servis henüz implement edilmedi"));
    }

    /// <summary>
    /// Create hobby
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] object hobbyDto)
    {
        // TODO: Implement service
        return Ok(ApiResponse<object>.ErrorResponse("Servis henüz implement edilmedi"));
    }

    /// <summary>
    /// Delete hobby
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
