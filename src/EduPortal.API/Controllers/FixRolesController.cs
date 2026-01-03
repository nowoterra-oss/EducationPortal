using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EduPortal.Domain.Entities;
using EduPortal.Application.Common;
using EduPortal.Application.Services.Interfaces;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FixRolesController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<FixRolesController> _logger;

    public FixRolesController(
        UserManager<ApplicationUser> userManager,
        IPermissionService permissionService,
        ILogger<FixRolesController> logger)
    {
        _userManager = userManager;
        _permissionService = permissionService;
        _logger = logger;
    }

    [HttpPost("assign-test-user-roles")]
    public async Task<ActionResult<ApiResponse<List<string>>>> AssignTestUserRoles()
    {
        var results = new List<string>();

        var testUsers = new Dictionary<string, string>
        {
            { "student@eduportal.com", "Ogrenci" },
            { "teacher@eduportal.com", "Ogretmen" },
            { "parent@eduportal.com", "Veli" },
            { "admin@eduportal.com", "Admin" }
        };

        foreach (var (email, roleName) in testUsers)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                results.Add($"❌ {email}: User not found");
                continue;
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Contains(roleName))
            {
                results.Add($"✓ {email}: Already has role '{roleName}'");
                continue;
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                results.Add($"✅ {email}: Role '{roleName}' added successfully");
                _logger.LogInformation($"Role '{roleName}' added to user {email}");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                results.Add($"❌ {email}: Failed to add role - {errors}");
                _logger.LogError($"Failed to add role '{roleName}' to user {email}: {errors}");
            }
        }

        return Ok(ApiResponse<List<string>>.SuccessResponse(results, "Role assignment completed"));
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<string>>> ResetPassword([FromQuery] string email, [FromQuery] string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return NotFound(ApiResponse<string>.ErrorResponse($"User not found: {email}"));
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (result.Succeeded)
        {
            _logger.LogInformation($"Password reset for user {email}");
            return Ok(ApiResponse<string>.SuccessResponse($"Password reset successfully for {email}"));
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        return BadRequest(ApiResponse<string>.ErrorResponse($"Failed to reset password: {errors}"));
    }

    [HttpPost("assign-parent-permissions")]
    public async Task<ActionResult<ApiResponse<bool>>> AssignParentPermissions([FromQuery] string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse($"User not found: {email}"));
        }

        var result = await _permissionService.AssignDefaultPermissionsToUserAsync(user.Id, "Parent");
        return Ok(result);
    }
}
