namespace EduPortal.Application.DTOs.Permission;

public class PermissionDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
