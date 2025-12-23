using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Permission;
using EduPortal.Application.Services.Interfaces;
using EduPortal.Domain.Constants;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public PermissionService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    #region Permission CRUD

    public async Task<List<PermissionDto>> GetAllPermissionsAsync()
    {
        return await _context.Permissions
            .Where(p => p.IsActive && !p.IsDeleted)
            .OrderBy(p => p.Category)
            .ThenBy(p => p.DisplayOrder)
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                Category = p.Category,
                Icon = p.Icon,
                DisplayOrder = p.DisplayOrder,
                IsActive = p.IsActive
            })
            .ToListAsync();
    }

    public async Task<Dictionary<string, List<PermissionDto>>> GetPermissionsByCategoryAsync()
    {
        var permissions = await GetAllPermissionsAsync();
        return permissions
            .GroupBy(p => p.Category)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    public async Task<PermissionDto?> GetPermissionByCodeAsync(string code)
    {
        return await _context.Permissions
            .Where(p => p.Code == code && !p.IsDeleted)
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                Category = p.Category,
                Icon = p.Icon,
                DisplayOrder = p.DisplayOrder,
                IsActive = p.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PermissionDto?> GetPermissionByIdAsync(int id)
    {
        return await _context.Permissions
            .Where(p => p.Id == id && !p.IsDeleted)
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                Category = p.Category,
                Icon = p.Icon,
                DisplayOrder = p.DisplayOrder,
                IsActive = p.IsActive
            })
            .FirstOrDefaultAsync();
    }

    #endregion

    #region User Permissions

    public async Task<UserPermissionsResponseDto> GetUserPermissionsAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new UserPermissionsResponseDto { UserId = userId };
        }

        var roles = await _userManager.GetRolesAsync(user);

        // Get all direct user permissions (both granted and denied)
        var allDirectPermissions = await _context.UserPermissions
            .Include(up => up.Permission)
            .Where(up => up.UserId == userId && !up.IsDeleted &&
                        (up.ExpiresAt == null || up.ExpiresAt > DateTime.UtcNow))
            .ToListAsync();

        // Separate granted and denied permissions
        var directPermissions = allDirectPermissions
            .Where(up => up.IsGranted)
            .Select(up => new PermissionDto
            {
                Id = up.Permission.Id,
                Code = up.Permission.Code,
                Name = up.Permission.Name,
                Description = up.Permission.Description,
                Category = up.Permission.Category,
                Icon = up.Permission.Icon,
                DisplayOrder = up.Permission.DisplayOrder,
                IsActive = up.Permission.IsActive
            })
            .ToList();

        var deniedPermissions = allDirectPermissions
            .Where(up => !up.IsGranted)
            .Select(up => new PermissionDto
            {
                Id = up.Permission.Id,
                Code = up.Permission.Code,
                Name = up.Permission.Name,
                Description = up.Permission.Description,
                Category = up.Permission.Category,
                Icon = up.Permission.Icon,
                DisplayOrder = up.Permission.DisplayOrder,
                IsActive = up.Permission.IsActive
            })
            .ToList();

        var deniedCodes = deniedPermissions.Select(p => p.Code).ToHashSet();

        // Role-based permissions
        var roleIds = await _context.Roles
            .Where(r => roles.Contains(r.Name!))
            .Select(r => r.Id)
            .ToListAsync();

        var rolePermissions = await _context.RolePermissions
            .Include(rp => rp.Permission)
            .Where(rp => roleIds.Contains(rp.RoleId) && !rp.IsDeleted)
            .Select(rp => new PermissionDto
            {
                Id = rp.Permission.Id,
                Code = rp.Permission.Code,
                Name = rp.Permission.Name,
                Description = rp.Permission.Description,
                Category = rp.Permission.Category,
                Icon = rp.Permission.Icon,
                DisplayOrder = rp.Permission.DisplayOrder,
                IsActive = rp.Permission.IsActive
            })
            .Distinct()
            .ToListAsync();

        // Effective permissions: (direct granted + role permissions) - denied
        var effectivePermissions = directPermissions
            .Select(p => p.Code)
            .Union(rolePermissions.Select(p => p.Code))
            .Except(deniedCodes)
            .Distinct()
            .ToList();

        return new UserPermissionsResponseDto
        {
            UserId = userId,
            UserName = user.UserName,
            UserEmail = user.Email,
            FullName = $"{user.FirstName} {user.LastName}",
            Roles = roles.ToList(),
            DirectPermissions = directPermissions,
            DeniedPermissions = deniedPermissions,
            RolePermissions = rolePermissions,
            EffectivePermissions = effectivePermissions
        };
    }

    public async Task<ApiResponse<bool>> AssignPermissionsToUserAsync(string grantedByUserId, AssignPermissionDto dto)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                return ApiResponse<bool>.ErrorResponse("Kullanıcı bulunamadı");
            }

            // Get ALL existing permissions for this user (including soft-deleted ones)
            var allExistingPermissions = await _context.UserPermissions
                .Where(up => up.UserId == dto.UserId)
                .ToListAsync();

            var newPermissionIds = dto.PermissionIds.ToHashSet();

            // Process each existing permission
            foreach (var existing in allExistingPermissions)
            {
                if (newPermissionIds.Contains(existing.PermissionId))
                {
                    // This permission should be active - reactivate if soft-deleted
                    existing.IsDeleted = false;
                    existing.IsGranted = true;
                    existing.ExpiresAt = dto.ExpiresAt;
                    existing.Notes = dto.Notes;
                    existing.GrantedByUserId = grantedByUserId;
                    existing.GrantedAt = DateTime.UtcNow;
                    existing.UpdatedAt = DateTime.UtcNow;

                    // Remove from set so we don't create a duplicate
                    newPermissionIds.Remove(existing.PermissionId);
                }
                else if (!existing.IsDeleted)
                {
                    // This permission is not in the new list - soft delete it
                    existing.IsDeleted = true;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
            }

            // Add new permissions that don't exist yet
            foreach (var permissionId in newPermissionIds)
            {
                var permission = await _context.Permissions.FindAsync(permissionId);
                if (permission == null) continue;

                var userPermission = new UserPermission
                {
                    UserId = dto.UserId,
                    PermissionId = permissionId,
                    IsGranted = true,
                    ExpiresAt = dto.ExpiresAt,
                    GrantedByUserId = grantedByUserId,
                    GrantedAt = DateTime.UtcNow,
                    Notes = dto.Notes
                };
                await _context.UserPermissions.AddAsync(userPermission);
            }

            await _context.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, "Yetkiler başarıyla atandı");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse($"Yetki atama hatası: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DenyPermissionToUserAsync(string deniedByUserId, string userId, int permissionId, string? notes = null)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<bool>.ErrorResponse("Kullanıcı bulunamadı");
            }

            var permission = await _context.Permissions.FindAsync(permissionId);
            if (permission == null)
            {
                return ApiResponse<bool>.ErrorResponse("Yetki bulunamadı");
            }

            // Check if permission record already exists (including soft-deleted)
            var existingPermission = await _context.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId);

            if (existingPermission != null)
            {
                // Update existing record to denied
                existingPermission.IsGranted = false;
                existingPermission.IsDeleted = false;
                existingPermission.GrantedByUserId = deniedByUserId;
                existingPermission.GrantedAt = DateTime.UtcNow;
                existingPermission.UpdatedAt = DateTime.UtcNow;
                existingPermission.Notes = notes ?? "Yetki engellendi";
            }
            else
            {
                // Create new denied permission record
                var userPermission = new UserPermission
                {
                    UserId = userId,
                    PermissionId = permissionId,
                    IsGranted = false, // DENIED
                    GrantedByUserId = deniedByUserId,
                    GrantedAt = DateTime.UtcNow,
                    Notes = notes ?? "Yetki engellendi"
                };
                await _context.UserPermissions.AddAsync(userPermission);
            }

            await _context.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, "Yetki başarıyla engellendi");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse($"Yetki engelleme hatası: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> RevokePermissionFromUserAsync(string userId, int permissionId)
    {
        try
        {
            var userPermission = await _context.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId && !up.IsDeleted);

            if (userPermission == null)
            {
                return ApiResponse<bool>.ErrorResponse("Yetki bulunamadı");
            }

            userPermission.IsDeleted = true;
            userPermission.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, "Yetki başarıyla kaldırıldı");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse($"Yetki kaldırma hatası: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> RevokeAllPermissionsFromUserAsync(string userId)
    {
        try
        {
            var userPermissions = await _context.UserPermissions
                .Where(up => up.UserId == userId && !up.IsDeleted)
                .ToListAsync();

            foreach (var up in userPermissions)
            {
                up.IsDeleted = true;
                up.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, "Tüm yetkiler başarıyla kaldırıldı");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse($"Yetki kaldırma hatası: {ex.Message}");
        }
    }

    #endregion

    #region Role Permissions

    public async Task<List<PermissionDto>> GetRolePermissionsAsync(string roleId)
    {
        return await _context.RolePermissions
            .Include(rp => rp.Permission)
            .Where(rp => rp.RoleId == roleId && !rp.IsDeleted)
            .Select(rp => new PermissionDto
            {
                Id = rp.Permission.Id,
                Code = rp.Permission.Code,
                Name = rp.Permission.Name,
                Description = rp.Permission.Description,
                Category = rp.Permission.Category,
                Icon = rp.Permission.Icon,
                DisplayOrder = rp.Permission.DisplayOrder,
                IsActive = rp.Permission.IsActive
            })
            .ToListAsync();
    }

    public async Task<ApiResponse<bool>> AssignPermissionsToRoleAsync(AssignRolePermissionDto dto)
    {
        try
        {
            var role = await _roleManager.FindByIdAsync(dto.RoleId);
            if (role == null)
            {
                return ApiResponse<bool>.ErrorResponse("Rol bulunamadı");
            }

            var existingRolePermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == dto.RoleId && !rp.IsDeleted)
                .ToListAsync();

            foreach (var permissionId in dto.PermissionIds)
            {
                var permission = await _context.Permissions.FindAsync(permissionId);
                if (permission == null) continue;

                var existing = existingRolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
                if (existing == null)
                {
                    var rolePermission = new RolePermission
                    {
                        RoleId = dto.RoleId,
                        PermissionId = permissionId
                    };
                    await _context.RolePermissions.AddAsync(rolePermission);
                }
            }

            await _context.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, "Rol yetkileri başarıyla atandı");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse($"Rol yetki atama hatası: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> RevokePermissionFromRoleAsync(string roleId, int permissionId)
    {
        try
        {
            var rolePermission = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && !rp.IsDeleted);

            if (rolePermission == null)
            {
                return ApiResponse<bool>.ErrorResponse("Rol yetkisi bulunamadı");
            }

            rolePermission.IsDeleted = true;
            rolePermission.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, "Rol yetkisi başarıyla kaldırıldı");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse($"Rol yetki kaldırma hatası: {ex.Message}");
        }
    }

    #endregion

    #region Permission Check

    public async Task<bool> HasPermissionAsync(string userId, string permissionCode)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        // Admin always has all permissions
        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        if (isAdmin) return true;

        // Check direct user permissions first (they override role permissions)
        var directPermission = await _context.UserPermissions
            .Include(up => up.Permission)
            .FirstOrDefaultAsync(up => up.UserId == userId &&
                           up.Permission.Code == permissionCode &&
                           !up.IsDeleted &&
                           (up.ExpiresAt == null || up.ExpiresAt > DateTime.UtcNow));

        // If direct permission exists, it overrides role permissions
        if (directPermission != null)
        {
            return directPermission.IsGranted; // true = granted, false = denied
        }

        // No direct permission found, check role-based permissions
        var roles = await _userManager.GetRolesAsync(user);
        var roleIds = await _context.Roles
            .Where(r => roles.Contains(r.Name!))
            .Select(r => r.Id)
            .ToListAsync();

        var hasRolePermission = await _context.RolePermissions
            .Include(rp => rp.Permission)
            .AnyAsync(rp => roleIds.Contains(rp.RoleId) &&
                           rp.Permission.Code == permissionCode &&
                           !rp.IsDeleted);

        return hasRolePermission;
    }

    public async Task<List<string>> GetEffectivePermissionsAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return new List<string>();

        // Admin has all permissions
        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        if (isAdmin)
        {
            return await _context.Permissions
                .Where(p => p.IsActive && !p.IsDeleted)
                .Select(p => p.Code)
                .ToListAsync();
        }

        // Get all direct user permissions (both granted and denied)
        var directUserPermissions = await _context.UserPermissions
            .Include(up => up.Permission)
            .Where(up => up.UserId == userId &&
                        !up.IsDeleted &&
                        (up.ExpiresAt == null || up.ExpiresAt > DateTime.UtcNow))
            .ToListAsync();

        // Separate granted and denied permissions
        var grantedDirectPermissions = directUserPermissions
            .Where(up => up.IsGranted)
            .Select(up => up.Permission.Code)
            .ToHashSet();

        var deniedPermissions = directUserPermissions
            .Where(up => !up.IsGranted)
            .Select(up => up.Permission.Code)
            .ToHashSet();

        // Role-based permissions
        var roles = await _userManager.GetRolesAsync(user);
        var roleIds = await _context.Roles
            .Where(r => roles.Contains(r.Name!))
            .Select(r => r.Id)
            .ToListAsync();

        var rolePermissions = await _context.RolePermissions
            .Include(rp => rp.Permission)
            .Where(rp => roleIds.Contains(rp.RoleId) && !rp.IsDeleted)
            .Select(rp => rp.Permission.Code)
            .ToListAsync();

        // Combine: (direct granted + role permissions) - denied permissions
        var effectivePermissions = grantedDirectPermissions
            .Union(rolePermissions)
            .Except(deniedPermissions)
            .Distinct()
            .ToList();

        return effectivePermissions;
    }

    #endregion

    #region Bulk Operations

    public async Task<ApiResponse<bool>> CopyPermissionsFromUserAsync(CopyPermissionsDto dto)
    {
        try
        {
            var sourceUser = await _userManager.FindByIdAsync(dto.SourceUserId);
            var targetUser = await _userManager.FindByIdAsync(dto.TargetUserId);

            if (sourceUser == null || targetUser == null)
            {
                return ApiResponse<bool>.ErrorResponse("Kaynak veya hedef kullanıcı bulunamadı");
            }

            var sourcePermissions = await _context.UserPermissions
                .Where(up => up.UserId == dto.SourceUserId && up.IsGranted && !up.IsDeleted)
                .ToListAsync();

            var existingTargetPermissions = await _context.UserPermissions
                .Where(up => up.UserId == dto.TargetUserId && !up.IsDeleted)
                .Select(up => up.PermissionId)
                .ToListAsync();

            foreach (var sourcePermission in sourcePermissions)
            {
                if (!existingTargetPermissions.Contains(sourcePermission.PermissionId))
                {
                    var newPermission = new UserPermission
                    {
                        UserId = dto.TargetUserId,
                        PermissionId = sourcePermission.PermissionId,
                        IsGranted = true,
                        ExpiresAt = sourcePermission.ExpiresAt,
                        GrantedByUserId = sourcePermission.GrantedByUserId,
                        GrantedAt = DateTime.UtcNow,
                        Notes = $"Kopyalandı: {sourceUser.Email}"
                    };
                    await _context.UserPermissions.AddAsync(newPermission);
                }
            }

            await _context.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, "Yetkiler başarıyla kopyalandı");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse($"Yetki kopyalama hatası: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> BulkUpdatePermissionsAsync(string updatedByUserId, string userId, BulkPermissionUpdateDto dto)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<bool>.ErrorResponse("Kullanıcı bulunamadı");
            }

            // Get ALL existing permissions for this user (including soft-deleted ones)
            var allExistingPermissions = await _context.UserPermissions
                .Where(up => up.UserId == userId)
                .ToListAsync();

            var grantedIds = dto.GrantedPermissionIds.ToHashSet();
            var deniedIds = dto.DeniedPermissionIds.ToHashSet();

            // Validate that no permission is both granted and denied
            var conflictIds = grantedIds.Intersect(deniedIds).ToList();
            if (conflictIds.Any())
            {
                return ApiResponse<bool>.ErrorResponse($"Aynı yetki hem verilemez hem engellenemez. Çakışan ID'ler: {string.Join(", ", conflictIds)}");
            }

            // Process each existing permission
            foreach (var existing in allExistingPermissions)
            {
                if (grantedIds.Contains(existing.PermissionId))
                {
                    // Grant this permission
                    existing.IsDeleted = false;
                    existing.IsGranted = true;
                    existing.GrantedByUserId = updatedByUserId;
                    existing.GrantedAt = DateTime.UtcNow;
                    existing.UpdatedAt = DateTime.UtcNow;
                    existing.Notes = dto.Notes ?? "Toplu güncelleme - Yetki verildi";
                    grantedIds.Remove(existing.PermissionId);
                }
                else if (deniedIds.Contains(existing.PermissionId))
                {
                    // Deny this permission
                    existing.IsDeleted = false;
                    existing.IsGranted = false;
                    existing.GrantedByUserId = updatedByUserId;
                    existing.GrantedAt = DateTime.UtcNow;
                    existing.UpdatedAt = DateTime.UtcNow;
                    existing.Notes = dto.Notes ?? "Toplu güncelleme - Yetki engellendi";
                    deniedIds.Remove(existing.PermissionId);
                }
                else if (!existing.IsDeleted)
                {
                    // This permission is not in either list - soft delete it
                    existing.IsDeleted = true;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
            }

            // Add new granted permissions that don't exist yet
            foreach (var permissionId in grantedIds)
            {
                var permission = await _context.Permissions.FindAsync(permissionId);
                if (permission == null) continue;

                var userPermission = new UserPermission
                {
                    UserId = userId,
                    PermissionId = permissionId,
                    IsGranted = true,
                    GrantedByUserId = updatedByUserId,
                    GrantedAt = DateTime.UtcNow,
                    Notes = dto.Notes ?? "Toplu güncelleme - Yetki verildi"
                };
                await _context.UserPermissions.AddAsync(userPermission);
            }

            // Add new denied permissions that don't exist yet
            foreach (var permissionId in deniedIds)
            {
                var permission = await _context.Permissions.FindAsync(permissionId);
                if (permission == null) continue;

                var userPermission = new UserPermission
                {
                    UserId = userId,
                    PermissionId = permissionId,
                    IsGranted = false,
                    GrantedByUserId = updatedByUserId,
                    GrantedAt = DateTime.UtcNow,
                    Notes = dto.Notes ?? "Toplu güncelleme - Yetki engellendi"
                };
                await _context.UserPermissions.AddAsync(userPermission);
            }

            await _context.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, "Yetkiler başarıyla güncellendi");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse($"Toplu yetki güncelleme hatası: {ex.Message}");
        }
    }

    #endregion

    #region Sync Permissions

    public async Task SyncPermissionsAsync()
    {
        var existingPermissions = await _context.Permissions
            .Where(p => !p.IsDeleted)
            .ToDictionaryAsync(p => p.Code, p => p);

        foreach (var (code, info) in Permissions.All)
        {
            if (existingPermissions.TryGetValue(code, out var existing))
            {
                // Update existing
                existing.Name = info.Name;
                existing.Category = info.Category;
                existing.Icon = info.Icon;
                existing.DisplayOrder = info.Order;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new
                var permission = new Permission
                {
                    Code = code,
                    Name = info.Name,
                    Category = info.Category,
                    Icon = info.Icon,
                    DisplayOrder = info.Order,
                    IsActive = true
                };
                await _context.Permissions.AddAsync(permission);
            }
        }

        await _context.SaveChangesAsync();
    }

    #endregion
}
