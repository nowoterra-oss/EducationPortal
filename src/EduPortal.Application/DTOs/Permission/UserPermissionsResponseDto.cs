namespace EduPortal.Application.DTOs.Permission;

public class UserPermissionsResponseDto
{
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public string? FullName { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<PermissionDto> DirectPermissions { get; set; } = new(); // Doğrudan atanan (granted)
    public List<PermissionDto> DeniedPermissions { get; set; } = new(); // Doğrudan engellenen (denied - role yetkisini override eder)
    public List<PermissionDto> RolePermissions { get; set; } = new(); // Role'den gelen
    public List<string> EffectivePermissions { get; set; } = new(); // Tüm etkin yetkiler (code listesi)
}
