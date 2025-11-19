using EduPortal.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Academic Development Plan (AGP - Akademik Gelişim Planı) endpoints
/// </summary>
[ApiController]
[Route("api/agp")]
[Produces("application/json")]
[Authorize]
public class AGPController : ControllerBase
{
    // TODO: Implement IAGPService
    private readonly ILogger<AGPController> _logger;

    public AGPController(ILogger<AGPController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get all AGP records with pagination
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Danışman")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<object>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<object>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        // TODO: Implement service
        return Ok(ApiResponse<PagedResponse<object>>.ErrorResponse("Servis henüz implement edilmedi"));
    }

    /// <summary>
    /// Get AGP by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> GetById(int id)
    {
        // TODO: Implement service
        return Ok(ApiResponse<object>.ErrorResponse("Servis henüz implement edilmedi"));
    }

    /// <summary>
    /// Create new AGP
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Danışman")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] object agpDto)
    {
        // TODO: Implement service
        return Ok(ApiResponse<object>.ErrorResponse("Servis henüz implement edilmedi"));
    }

    /// <summary>
    /// Update AGP
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Danışman")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Update(int id, [FromBody] object agpDto)
    {
        // TODO: Implement service
        return Ok(ApiResponse<object>.ErrorResponse("Servis henüz implement edilmedi"));
    }

    /// <summary>
    /// Delete AGP
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        // TODO: Implement service
        return Ok(ApiResponse<bool>.ErrorResponse("Servis henüz implement edilmedi"));
    }

    /// <summary>
    /// Get student AGP
    /// </summary>
    [HttpGet("student/{studentId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> GetByStudent(int studentId)
    {
        // TODO: Implement service
        return Ok(ApiResponse<object>.ErrorResponse("Servis henüz implement edilmedi"));
    }

    /// <summary>
    /// Get AGP goals
    /// </summary>
    [HttpGet("{id}/goals")]
    [ProducesResponseType(typeof(ApiResponse<List<object>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetGoals(int id)
    {
        // TODO: Implement service
        return Ok(ApiResponse<List<object>>.ErrorResponse("Servis henüz implement edilmedi"));
    }

    /// <summary>
    /// Add goal to AGP
    /// </summary>
    [HttpPost("{id}/goals")]
    [Authorize(Roles = "Admin,Danışman")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<object>>> AddGoal(int id, [FromBody] object goalDto)
    {
        // TODO: Implement service
        return Ok(ApiResponse<object>.ErrorResponse("Servis henüz implement edilmedi"));
    }

    /// <summary>
    /// Update goal
    /// </summary>
    [HttpPut("{id}/goals/{goalId}")]
    [Authorize(Roles = "Admin,Danışman")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> UpdateGoal(int id, int goalId, [FromBody] object goalDto)
    {
        // TODO: Implement service
        return Ok(ApiResponse<object>.ErrorResponse("Servis henüz implement edilmedi"));
    }

    /// <summary>
    /// Delete goal
    /// </summary>
    [HttpDelete("{id}/goals/{goalId}")]
    [Authorize(Roles = "Admin,Danışman")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteGoal(int id, int goalId)
    {
        // TODO: Implement service
        return Ok(ApiResponse<bool>.ErrorResponse("Servis henüz implement edilmedi"));
    }

    /// <summary>
    /// Get AGP progress
    /// </summary>
    [HttpGet("{id}/progress")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> GetProgress(int id)
    {
        // TODO: Implement service
        return Ok(ApiResponse<object>.ErrorResponse("Servis henüz implement edilmedi"));
    }
}
