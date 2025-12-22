using System.Security.Claims;
using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Permission;
using EduPortal.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers;

/// <summary>
/// Permission management endpoints
/// </summary>
[ApiController]
[Route("api/permissions")]
[Produces("application/json")]
[Authorize]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService _permissionService;
    private readonly ILogger<PermissionsController> _logger;

    public PermissionsController(IPermissionService permissionService, ILogger<PermissionsController> logger)
    {
        _permissionService = permissionService;
        _logger = logger;
    }

    /// <summary>
    /// Get all available permissions
    /// </summary>
    /// <returns>List of all permissions</returns>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<List<PermissionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<PermissionDto>>>> GetAll()
    {
        try
        {
            var permissions = await _permissionService.GetAllPermissionsAsync();
            return Ok(ApiResponse<List<PermissionDto>>.SuccessResponse(permissions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all permissions");
            return StatusCode(500, ApiResponse<List<PermissionDto>>.ErrorResponse("Yetkiler alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get permissions grouped by category
    /// </summary>
    /// <returns>Permissions grouped by category</returns>
    [HttpGet("categories")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<Dictionary<string, List<PermissionDto>>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<Dictionary<string, List<PermissionDto>>>>> GetByCategories()
    {
        try
        {
            var permissions = await _permissionService.GetPermissionsByCategoryAsync();
            return Ok(ApiResponse<Dictionary<string, List<PermissionDto>>>.SuccessResponse(permissions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions by categories");
            return StatusCode(500, ApiResponse<Dictionary<string, List<PermissionDto>>>.ErrorResponse("Yetkiler alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get a specific user's permissions
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User's permissions including direct and role-based</returns>
    [HttpGet("user/{userId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<UserPermissionsResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserPermissionsResponseDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserPermissionsResponseDto>>> GetUserPermissions(string userId)
    {
        try
        {
            var permissions = await _permissionService.GetUserPermissionsAsync(userId);
            return Ok(ApiResponse<UserPermissionsResponseDto>.SuccessResponse(permissions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user permissions for {UserId}", userId);
            return StatusCode(500, ApiResponse<UserPermissionsResponseDto>.ErrorResponse("Kullanıcı yetkileri alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Assign permissions to a user
    /// </summary>
    /// <param name="dto">Permission assignment details</param>
    /// <returns>Success status</returns>
    [HttpPost("assign")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> AssignPermissions([FromBody] AssignPermissionDto dto)
    {
        try
        {
            var grantedByUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(grantedByUserId))
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse("Kullanıcı kimliği alınamadı"));
            }

            var result = await _permissionService.AssignPermissionsToUserAsync(grantedByUserId, dto);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning permissions to user {UserId}", dto.UserId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Yetki atama sırasında bir hata oluştu"));
        }
    }

    /// <summary>
    /// Revoke a specific permission from a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="permissionId">Permission ID to revoke</param>
    /// <returns>Success status</returns>
    [HttpDelete("user/{userId}/permission/{permissionId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> RevokePermission(string userId, int permissionId)
    {
        try
        {
            var result = await _permissionService.RevokePermissionFromUserAsync(userId, permissionId);
            if (result.Success)
            {
                return Ok(result);
            }
            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking permission {PermissionId} from user {UserId}", permissionId, userId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Yetki kaldırma sırasında bir hata oluştu"));
        }
    }

    /// <summary>
    /// Revoke all permissions from a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("user/{userId}/all")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> RevokeAllPermissions(string userId)
    {
        try
        {
            var result = await _permissionService.RevokeAllPermissionsFromUserAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking all permissions from user {UserId}", userId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Yetki kaldırma sırasında bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get current user's effective permissions
    /// </summary>
    /// <returns>List of permission codes</returns>
    [HttpGet("my")]
    [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetMyPermissions()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(ApiResponse<List<string>>.ErrorResponse("Kullanıcı kimliği alınamadı"));
            }

            var permissions = await _permissionService.GetEffectivePermissionsAsync(userId);
            return Ok(ApiResponse<List<string>>.SuccessResponse(permissions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user's permissions");
            return StatusCode(500, ApiResponse<List<string>>.ErrorResponse("Yetkiler alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Copy permissions from one user to another
    /// </summary>
    /// <param name="dto">Source and target user IDs</param>
    /// <returns>Success status</returns>
    [HttpPost("copy")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> CopyPermissions([FromBody] CopyPermissionsDto dto)
    {
        try
        {
            var result = await _permissionService.CopyPermissionsFromUserAsync(dto);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying permissions from {Source} to {Target}", dto.SourceUserId, dto.TargetUserId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Yetki kopyalama sırasında bir hata oluştu"));
        }
    }

    /// <summary>
    /// Get permissions for a specific role
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <returns>List of permissions assigned to the role</returns>
    [HttpGet("role/{roleId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<List<PermissionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<PermissionDto>>>> GetRolePermissions(string roleId)
    {
        try
        {
            var permissions = await _permissionService.GetRolePermissionsAsync(roleId);
            return Ok(ApiResponse<List<PermissionDto>>.SuccessResponse(permissions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role permissions for {RoleId}", roleId);
            return StatusCode(500, ApiResponse<List<PermissionDto>>.ErrorResponse("Rol yetkileri alınırken bir hata oluştu"));
        }
    }

    /// <summary>
    /// Assign permissions to a role
    /// </summary>
    /// <param name="dto">Role permission assignment details</param>
    /// <returns>Success status</returns>
    [HttpPost("role/assign")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> AssignRolePermissions([FromBody] AssignRolePermissionDto dto)
    {
        try
        {
            var result = await _permissionService.AssignPermissionsToRoleAsync(dto);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning permissions to role {RoleId}", dto.RoleId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Rol yetki atama sırasında bir hata oluştu"));
        }
    }

    /// <summary>
    /// Revoke a permission from a role
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <param name="permissionId">Permission ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("role/{roleId}/permission/{permissionId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> RevokeRolePermission(string roleId, int permissionId)
    {
        try
        {
            var result = await _permissionService.RevokePermissionFromRoleAsync(roleId, permissionId);
            if (result.Success)
            {
                return Ok(result);
            }
            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking permission {PermissionId} from role {RoleId}", permissionId, roleId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Rol yetki kaldırma sırasında bir hata oluştu"));
        }
    }

    /// <summary>
    /// Check if current user has a specific permission
    /// </summary>
    /// <param name="permissionCode">Permission code to check</param>
    /// <returns>True if user has the permission</returns>
    [HttpGet("check/{permissionCode}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> CheckPermission(string permissionCode)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse("Kullanıcı kimliği alınamadı"));
            }

            var hasPermission = await _permissionService.HasPermissionAsync(userId, permissionCode);
            return Ok(ApiResponse<bool>.SuccessResponse(hasPermission));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {PermissionCode}", permissionCode);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Yetki kontrolü sırasında bir hata oluştu"));
        }
    }

    /// <summary>
    /// Sync permissions from constants to database
    /// </summary>
    /// <returns>Success status</returns>
    [HttpPost("sync")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> SyncPermissions()
    {
        try
        {
            await _permissionService.SyncPermissionsAsync();
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Yetkiler başarıyla senkronize edildi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing permissions");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Yetki senkronizasyonu sırasında bir hata oluştu"));
        }
    }
}
