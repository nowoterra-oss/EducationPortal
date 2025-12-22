namespace EduPortal.Application.DTOs.Permission;

public class UserPermissionDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public int PermissionId { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public string PermissionName { get; set; } = string.Empty;
    public string PermissionCategory { get; set; } = string.Empty;
    public bool IsGranted { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? GrantedByUserName { get; set; }
    public DateTime GrantedAt { get; set; }
    public string? Notes { get; set; }
}
