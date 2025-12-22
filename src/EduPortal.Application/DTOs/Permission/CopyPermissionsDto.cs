namespace EduPortal.Application.DTOs.Permission;

public class CopyPermissionsDto
{
    public string SourceUserId { get; set; } = string.Empty;
    public string TargetUserId { get; set; } = string.Empty;
}
