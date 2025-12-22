namespace EduPortal.Application.DTOs.Permission;

public class AssignPermissionDto
{
    public string UserId { get; set; } = string.Empty;
    public List<int> PermissionIds { get; set; } = new();
    public DateTime? ExpiresAt { get; set; }
    public string? Notes { get; set; }
}
