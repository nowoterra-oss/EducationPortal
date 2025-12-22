using System.Security.Claims;
using EduPortal.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EduPortal.API.Attributes;

/// <summary>
/// Authorization attribute that checks for specific permissions
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute, IAsyncAuthorizationFilter
{
    public string Permission { get; }
    public string[]? Permissions { get; }
    public bool RequireAll { get; set; } = false;

    /// <summary>
    /// Requires a single permission
    /// </summary>
    /// <param name="permission">The permission code required</param>
    public RequirePermissionAttribute(string permission)
    {
        Permission = permission;
        Permissions = null;
    }

    /// <summary>
    /// Requires multiple permissions
    /// </summary>
    /// <param name="permissions">Array of permission codes required</param>
    public RequirePermissionAttribute(params string[] permissions)
    {
        Permission = permissions.FirstOrDefault() ?? string.Empty;
        Permissions = permissions;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        // Check if user is authenticated
        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Admin always has all permissions (bypass)
        if (user.IsInRole("Admin"))
        {
            return;
        }

        var permissionService = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        bool hasPermission;

        if (Permissions != null && Permissions.Length > 1)
        {
            // Multiple permissions
            if (RequireAll)
            {
                // User must have ALL permissions
                hasPermission = true;
                foreach (var perm in Permissions)
                {
                    if (!await permissionService.HasPermissionAsync(userId, perm))
                    {
                        hasPermission = false;
                        break;
                    }
                }
            }
            else
            {
                // User must have ANY of the permissions
                hasPermission = false;
                foreach (var perm in Permissions)
                {
                    if (await permissionService.HasPermissionAsync(userId, perm))
                    {
                        hasPermission = true;
                        break;
                    }
                }
            }
        }
        else
        {
            // Single permission
            hasPermission = await permissionService.HasPermissionAsync(userId, Permission);
        }

        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }
    }
}
