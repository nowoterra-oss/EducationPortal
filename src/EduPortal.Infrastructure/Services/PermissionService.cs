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

    #region Default Permissions

    public async Task<ApiResponse<bool>> AssignDefaultPermissionsToUserAsync(string userId, string userType)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<bool>.ErrorResponse("Kullanıcı bulunamadı");
            }

            // Önce permission'ları senkronize et (yeni eklenenler için)
            await SyncPermissionsAsync();

            // Kullanıcı tipine göre varsayılan yetki kodlarını al
            var defaultPermissionCodes = Permissions.GetDefaultPermissionsForUserType(userType);
            if (!defaultPermissionCodes.Any())
            {
                return ApiResponse<bool>.SuccessResponse(true, "Bu kullanıcı tipi için varsayılan yetki tanımlanmamış");
            }

            // Kodlara göre permission ID'lerini bul
            var permissions = await _context.Permissions
                .Where(p => defaultPermissionCodes.Contains(p.Code) && !p.IsDeleted && p.IsActive)
                .ToListAsync();

            if (!permissions.Any())
            {
                return ApiResponse<bool>.ErrorResponse("Varsayılan yetkiler veritabanında bulunamadı");
            }

            // Mevcut kullanıcı yetkilerini kontrol et
            var existingPermissionIds = await _context.UserPermissions
                .Where(up => up.UserId == userId && !up.IsDeleted)
                .Select(up => up.PermissionId)
                .ToListAsync();

            // Yeni yetkileri ekle
            foreach (var permission in permissions)
            {
                if (!existingPermissionIds.Contains(permission.Id))
                {
                    var userPermission = new UserPermission
                    {
                        UserId = userId,
                        PermissionId = permission.Id,
                        IsGranted = true,
                        GrantedAt = DateTime.UtcNow,
                        Notes = $"Varsayılan {userType} yetkileri otomatik atandı"
                    };
                    await _context.UserPermissions.AddAsync(userPermission);
                }
            }

            await _context.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, $"{permissions.Count} varsayılan yetki başarıyla atandı");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse($"Varsayılan yetki atama hatası: {ex.Message}");
        }
    }

    #endregion

    #region User Management for Permissions

    public async Task<UserSearchResultDto> GetAllUsersForPermissionAsync(string? searchTerm, string? userType, int pageNumber, int pageSize)
    {
        var result = new UserSearchResultDto();
        var users = new List<UserForPermissionDto>();
        var searchTermLower = searchTerm?.ToLower() ?? "";

        // Her tip için ayrı ayrı sorgula ve listeye ekle (AsNoTracking ile performans)
        // Admin kullanıcıları
        if (string.IsNullOrEmpty(userType) || userType == "Admin")
        {
            var adminQuery = from ur in _context.UserRoles.AsNoTracking()
                             join r in _context.Roles.AsNoTracking() on ur.RoleId equals r.Id
                             join u in _context.Users.AsNoTracking() on ur.UserId equals u.Id
                             where r.Name == "Admin"
                             select new { u.Id, u.FirstName, u.LastName, u.Email, u.PhoneNumber, u.IsActive };

            if (!string.IsNullOrEmpty(searchTermLower))
            {
                adminQuery = adminQuery.Where(u =>
                    (u.FirstName + " " + u.LastName).ToLower().Contains(searchTermLower) ||
                    (u.Email != null && u.Email.ToLower().Contains(searchTermLower)));
            }

            var admins = await adminQuery.ToListAsync();
            users.AddRange(admins.Select(u => new UserForPermissionDto
            {
                UserId = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email ?? "",
                PhoneNumber = u.PhoneNumber,
                UserType = "Admin",
                IsActive = u.IsActive
            }));
        }

        // Öğrenciler
        if (string.IsNullOrEmpty(userType) || userType == "Student")
        {
            var studentQuery = from s in _context.Students.AsNoTracking()
                               join u in _context.Users.AsNoTracking() on s.UserId equals u.Id
                               where !s.IsDeleted
                               select new { s.UserId, u.FirstName, u.LastName, u.Email, u.PhoneNumber, u.IsActive, s.Id, s.StudentNo, s.ProfilePhotoUrl };

            if (!string.IsNullOrEmpty(searchTermLower))
            {
                studentQuery = studentQuery.Where(x =>
                    (x.FirstName + " " + x.LastName).ToLower().Contains(searchTermLower) ||
                    (x.Email != null && x.Email.ToLower().Contains(searchTermLower)) ||
                    (x.StudentNo != null && x.StudentNo.ToLower().Contains(searchTermLower)));
            }

            var students = await studentQuery.ToListAsync();
            users.AddRange(students.Select(s => new UserForPermissionDto
            {
                UserId = s.UserId,
                FirstName = s.FirstName,
                LastName = s.LastName,
                Email = s.Email ?? "",
                PhoneNumber = s.PhoneNumber,
                UserType = "Student",
                EntityId = s.Id,
                EntityNo = s.StudentNo,
                IsActive = s.IsActive,
                ProfilePhotoUrl = s.ProfilePhotoUrl
            }));
        }

        // Öğretmenler
        if (string.IsNullOrEmpty(userType) || userType == "Teacher")
        {
            var teacherQuery = from t in _context.Teachers.AsNoTracking()
                               join u in _context.Users.AsNoTracking() on t.UserId equals u.Id
                               where !t.IsDeleted && t.IsActive
                               select new { t.UserId, u.FirstName, u.LastName, u.Email, u.PhoneNumber, t.Id, t.IsActive, t.ProfilePhotoUrl };

            if (!string.IsNullOrEmpty(searchTermLower))
            {
                teacherQuery = teacherQuery.Where(x =>
                    (x.FirstName + " " + x.LastName).ToLower().Contains(searchTermLower) ||
                    (x.Email != null && x.Email.ToLower().Contains(searchTermLower)));
            }

            var teachers = await teacherQuery.ToListAsync();
            users.AddRange(teachers.Select(t => new UserForPermissionDto
            {
                UserId = t.UserId,
                FirstName = t.FirstName,
                LastName = t.LastName,
                Email = t.Email ?? "",
                PhoneNumber = t.PhoneNumber,
                UserType = "Teacher",
                EntityId = t.Id,
                IsActive = t.IsActive,
                ProfilePhotoUrl = t.ProfilePhotoUrl
            }));
        }

        // Danışmanlar
        if (string.IsNullOrEmpty(userType) || userType == "Counselor")
        {
            var counselorQuery = from c in _context.Counselors.AsNoTracking()
                                 join u in _context.Users.AsNoTracking() on c.UserId equals u.Id
                                 where !c.IsDeleted && c.IsActive
                                 select new { c.UserId, u.FirstName, u.LastName, u.Email, u.PhoneNumber, c.Id, c.IsActive };

            if (!string.IsNullOrEmpty(searchTermLower))
            {
                counselorQuery = counselorQuery.Where(x =>
                    (x.FirstName + " " + x.LastName).ToLower().Contains(searchTermLower) ||
                    (x.Email != null && x.Email.ToLower().Contains(searchTermLower)));
            }

            var counselors = await counselorQuery.ToListAsync();
            // Öğretmen olarak zaten eklenenleri atla
            var existingUserIds = users.Select(u => u.UserId).ToHashSet();
            users.AddRange(counselors.Where(c => !existingUserIds.Contains(c.UserId)).Select(c => new UserForPermissionDto
            {
                UserId = c.UserId,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email ?? "",
                PhoneNumber = c.PhoneNumber,
                UserType = "Counselor",
                EntityId = c.Id,
                IsActive = c.IsActive
            }));
        }

        // Veliler
        if (string.IsNullOrEmpty(userType) || userType == "Parent")
        {
            var parentQuery = from p in _context.Parents.AsNoTracking()
                              join u in _context.Users.AsNoTracking() on p.UserId equals u.Id
                              where !p.IsDeleted
                              select new { p.UserId, u.FirstName, u.LastName, u.Email, u.PhoneNumber, p.Id };

            if (!string.IsNullOrEmpty(searchTermLower))
            {
                parentQuery = parentQuery.Where(x =>
                    (x.FirstName + " " + x.LastName).ToLower().Contains(searchTermLower) ||
                    (x.Email != null && x.Email.ToLower().Contains(searchTermLower)));
            }

            var parents = await parentQuery.ToListAsync();
            users.AddRange(parents.Select(p => new UserForPermissionDto
            {
                UserId = p.UserId,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Email = p.Email ?? "",
                PhoneNumber = p.PhoneNumber,
                UserType = "Parent",
                EntityId = p.Id,
                IsActive = true
            }));
        }

        // Duplicate'leri kaldır (bir kullanıcı birden fazla role sahip olabilir)
        users = users.GroupBy(u => u.UserId).Select(g => g.First()).ToList();

        result.TotalCount = users.Count;

        // Memory'de pagination ve sıralama
        var pagedUsers = users
            .OrderBy(u => u.UserType)
            .ThenBy(u => u.FullName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Sadece sayfalanmış kullanıcılar için permission'ları yükle (N+1 problemi çözümü)
        var userIds = pagedUsers.Select(u => u.UserId).ToList();
        var userPermissions = await GetEffectivePermissionsForUsersAsync(userIds);

        foreach (var user in pagedUsers)
        {
            if (userPermissions.TryGetValue(user.UserId, out var permissions))
            {
                user.CurrentPermissions = permissions;
            }
        }

        result.Users = pagedUsers;
        return result;
    }

    /// <summary>
    /// Birden fazla kullanıcı için SADECE direkt atanmış permission'ları tek sorguda getirir
    /// Rol bazlı yetkiler dahil edilmez - kullanıcı listesinde doğru yetki sayısı göstermek için
    /// </summary>
    private async Task<Dictionary<string, List<string>>> GetEffectivePermissionsForUsersAsync(List<string> userIds)
    {
        var result = new Dictionary<string, List<string>>();

        if (!userIds.Any())
            return result;

        // Sadece kullanıcıya doğrudan atanmış permission'ları getir (rol yetkileri hariç)
        var directPermissions = await (
            from up in _context.UserPermissions.AsNoTracking()
            join p in _context.Permissions.AsNoTracking() on up.PermissionId equals p.Id
            where userIds.Contains(up.UserId) && up.IsGranted && !up.IsDeleted && p.IsActive && !p.IsDeleted
            select new { up.UserId, p.Code }
        ).ToListAsync();

        // Her kullanıcı için sadece direkt permission listesi oluştur
        foreach (var userId in userIds)
        {
            var userDirectPermissions = directPermissions
                .Where(x => x.UserId == userId)
                .Select(x => x.Code)
                .Distinct()
                .ToList();

            result[userId] = userDirectPermissions;
        }

        return result;
    }

    public async Task<IEnumerable<PermissionModuleDto>> GetPermissionsByUserTypeAsync(string userType)
    {
        // Kullanıcı tipine göre ilgili kategorileri filtrele
        string[]? allowedCategories = userType switch
        {
            "Student" => new[] { "Öğrenci Paneli" },
            "Teacher" => new[] {
                "Öğrenci Yönetimi", "Ders Yönetimi", "Ders Planlama", "Ödevler",
                "Sınavlar", "Yoklama", "Duyurular", "Mesajlar", "Grup Dersleri",
                "Akademik Gelişim Planı", "Danışmanlık", "Danışman Paneli"
            },
            "Counselor" => new[] { "Danışman Paneli" },
            "Parent" => new[] { "Veli Paneli" },
            "Admin" => null, // Admin tüm modüllere erişebilir
            _ => new[] { "Genel" }
        };

        var permissionsQuery = _context.Permissions
            .Where(p => p.IsActive && !p.IsDeleted);

        if (allowedCategories != null)
        {
            permissionsQuery = permissionsQuery.Where(p => allowedCategories.Contains(p.Category));
        }

        var permissions = await permissionsQuery
            .OrderBy(p => p.Category)
            .ThenBy(p => p.DisplayOrder)
            .ToListAsync();

        return permissions
            .GroupBy(p => p.Category)
            .Select(g => new PermissionModuleDto
            {
                Module = g.Key,
                ModuleName = g.Key,
                Permissions = g.Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    Description = p.Description,
                    Category = p.Category,
                    Icon = p.Icon,
                    DisplayOrder = p.DisplayOrder,
                    IsActive = p.IsActive
                }).ToList()
            })
            .OrderBy(m => m.ModuleName)
            .ToList();
    }

    #endregion

    #region User Type Management

    public async Task<ApiResponse<bool>> UpdateUserTypeAsync(string userId, string newUserType)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<bool>.ErrorResponse("Kullanıcı bulunamadı");
            }

            // Admin kullanıcısının tipi değiştirilemez
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
            {
                return ApiResponse<bool>.ErrorResponse("Admin kullanıcısının tipi değiştirilemez");
            }

            // Geçerli kullanıcı tiplerini kontrol et
            var validTypes = new[] { "Student", "Teacher", "Counselor", "Parent", "Other" };
            if (!validTypes.Contains(newUserType))
            {
                return ApiResponse<bool>.ErrorResponse("Geçersiz kullanıcı tipi. Geçerli değerler: Student, Teacher, Counselor, Parent, Other");
            }

            // Mevcut kullanıcı tipini kontrol et
            var currentUserType = await GetUserTypeAsync(userId);

            // Aynı tip ise işlem yapma
            if (currentUserType == newUserType)
            {
                return ApiResponse<bool>.SuccessResponse(true, "Kullanıcı zaten bu tipte");
            }

            // Yeni tipe göre entity oluştur
            switch (newUserType)
            {
                case "Student":
                    // Öğrenci entity'si var mı kontrol et
                    var existingStudent = await _context.Students
                        .FirstOrDefaultAsync(s => s.UserId == userId && !s.IsDeleted);
                    if (existingStudent == null)
                    {
                        var studentNo = $"STD{DateTime.Now.Year}{DateTime.Now.Ticks % 10000:D4}";
                        var student = new Student
                        {
                            UserId = userId,
                            StudentNo = studentNo,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _context.Students.AddAsync(student);
                    }
                    break;

                case "Teacher":
                    var existingTeacher = await _context.Teachers
                        .FirstOrDefaultAsync(t => t.UserId == userId && !t.IsDeleted);
                    if (existingTeacher == null)
                    {
                        var teacher = new Teacher
                        {
                            UserId = userId,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _context.Teachers.AddAsync(teacher);
                    }
                    break;

                case "Counselor":
                    var existingCounselor = await _context.Counselors
                        .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);
                    if (existingCounselor == null)
                    {
                        var counselor = new Counselor
                        {
                            UserId = userId,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _context.Counselors.AddAsync(counselor);
                    }
                    break;

                case "Parent":
                    var existingParent = await _context.Parents
                        .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);
                    if (existingParent == null)
                    {
                        var parent = new Parent
                        {
                            UserId = userId,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _context.Parents.AddAsync(parent);
                    }
                    break;

                case "Other":
                    // Other tipi için entity oluşturulmuyor
                    break;
            }

            await _context.SaveChangesAsync();

            // Yeni tipe göre varsayılan yetkileri ata
            if (newUserType != "Other")
            {
                await AssignDefaultPermissionsToUserAsync(userId, newUserType);
            }

            return ApiResponse<bool>.SuccessResponse(true, "Kullanıcı tipi başarıyla güncellendi");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse($"Kullanıcı tipi güncellenirken hata oluştu: {ex.Message}");
        }
    }

    public async Task<string> GetUserTypeAsync(string userId)
    {
        // Admin kontrolü
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return "Unknown";

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("Admin")) return "Admin";

        // Entity tablolarından kontrol
        var isStudent = await _context.Students.AnyAsync(s => s.UserId == userId && !s.IsDeleted);
        if (isStudent) return "Student";

        var isTeacher = await _context.Teachers.AnyAsync(t => t.UserId == userId && !t.IsDeleted);
        if (isTeacher) return "Teacher";

        var isCounselor = await _context.Counselors.AnyAsync(c => c.UserId == userId && !c.IsDeleted);
        if (isCounselor) return "Counselor";

        var isParent = await _context.Parents.AnyAsync(p => p.UserId == userId && !p.IsDeleted);
        if (isParent) return "Parent";

        return "Other";
    }

    #endregion
}
