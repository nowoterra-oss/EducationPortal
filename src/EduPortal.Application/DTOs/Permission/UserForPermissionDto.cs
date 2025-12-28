namespace EduPortal.Application.DTOs.Permission;

public class UserForPermissionDto
{
    public string UserId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string UserType { get; set; } = string.Empty; // "Student", "Teacher", "Parent", "Counselor", "Admin", "Other"
    public int? EntityId { get; set; } // StudentId, TeacherId, etc.
    public string? EntityNo { get; set; } // StudentNo, etc.
    public bool IsActive { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public List<string> CurrentPermissions { get; set; } = new();
}

public class UserSearchResultDto
{
    public List<UserForPermissionDto> Users { get; set; } = new();
    public int TotalCount { get; set; }
}

public class PermissionModuleDto
{
    public string Module { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public List<PermissionDto> Permissions { get; set; } = new();
}
