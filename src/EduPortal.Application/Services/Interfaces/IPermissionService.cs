using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Permission;

namespace EduPortal.Application.Services.Interfaces;

public interface IPermissionService
{
    // Permission CRUD
    Task<List<PermissionDto>> GetAllPermissionsAsync();
    Task<Dictionary<string, List<PermissionDto>>> GetPermissionsByCategoryAsync();
    Task<PermissionDto?> GetPermissionByCodeAsync(string code);
    Task<PermissionDto?> GetPermissionByIdAsync(int id);

    // User Permissions
    Task<UserPermissionsResponseDto> GetUserPermissionsAsync(string userId);
    Task<ApiResponse<bool>> AssignPermissionsToUserAsync(string grantedByUserId, AssignPermissionDto dto);
    Task<ApiResponse<bool>> DenyPermissionToUserAsync(string deniedByUserId, string userId, int permissionId, string? notes = null);
    Task<ApiResponse<bool>> RevokePermissionFromUserAsync(string userId, int permissionId);
    Task<ApiResponse<bool>> RevokeAllPermissionsFromUserAsync(string userId);

    // Role Permissions
    Task<List<PermissionDto>> GetRolePermissionsAsync(string roleId);
    Task<ApiResponse<bool>> AssignPermissionsToRoleAsync(AssignRolePermissionDto dto);
    Task<ApiResponse<bool>> RevokePermissionFromRoleAsync(string roleId, int permissionId);

    // Permission Check
    Task<bool> HasPermissionAsync(string userId, string permissionCode);
    Task<List<string>> GetEffectivePermissionsAsync(string userId);

    // Bulk Operations
    Task<ApiResponse<bool>> CopyPermissionsFromUserAsync(CopyPermissionsDto dto);
    Task<ApiResponse<bool>> BulkUpdatePermissionsAsync(string updatedByUserId, string userId, BulkPermissionUpdateDto dto);

    // Sync permissions from constants to database
    Task SyncPermissionsAsync();
}
