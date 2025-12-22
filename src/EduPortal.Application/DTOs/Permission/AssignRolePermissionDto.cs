namespace EduPortal.Application.DTOs.Permission;

public class AssignRolePermissionDto
{
    public string RoleId { get; set; } = string.Empty;
    public List<int> PermissionIds { get; set; } = new();
}
