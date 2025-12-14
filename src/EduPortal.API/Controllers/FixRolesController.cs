using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EduPortal.Domain.Entities;
using EduPortal.Application.Common;

namespace EduPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FixRolesController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<FixRolesController> _logger;

    public FixRolesController(UserManager<ApplicationUser> userManager, ILogger<FixRolesController> logger)
    {
        _userManager = userManager;
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
}
