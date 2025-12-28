using System.Security.Claims;
using EduPortal.Application.Common;
using EduPortal.Application.DTOs.User;
using EduPortal.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// User management endpoints
/// </summary>
[ApiController]
[Route("api/users")]
[Produces("application/json")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IPermissionService _permissionService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IPermissionService permissionService, ILogger<UsersController> logger)
    {
        _permissionService = permissionService;
        _logger = logger;
    }

    /// <summary>
    /// Update user type
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="dto">New user type</param>
    /// <returns>Success status</returns>
    /// <remarks>
    /// Valid user types: Student, Teacher, Counselor, Parent, Other
    /// Admin users cannot have their type changed.
    /// When type changes, the corresponding entity record is created if it doesn't exist.
    /// </remarks>
    [HttpPut("{userId}/type")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateUserType(string userId, [FromBody] UpdateUserTypeDto dto)
    {
        try
        {
            var result = await _permissionService.UpdateUserTypeAsync(userId, dto.UserType);

            if (result.Success)
            {
                return Ok(result);
            }

            // Hata mesajına göre uygun HTTP status code döndür
            if (result.Message?.Contains("bulunamadı") == true)
            {
                return NotFound(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user type for {UserId}", userId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Kullanıcı tipi güncellenirken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get user type
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User type (Admin, Student, Teacher, Counselor, Parent, Other, Unknown)</returns>
    [HttpGet("{userId}/type")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<string>>> GetUserType(string userId)
    {
        try
        {
            var userType = await _permissionService.GetUserTypeAsync(userId);
            return Ok(ApiResponse<string>.SuccessResponse(userType));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user type for {UserId}", userId);
            return StatusCode(500, ApiResponse<string>.ErrorResponse("Kullanıcı tipi alınırken bir hata oluştu"));
        }
    }
}
