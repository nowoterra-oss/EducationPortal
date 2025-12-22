using EduPortal.Domain.Common;

namespace EduPortal.Domain.Entities;

public class Permission : BaseEntity
{
    public string Code { get; set; } = string.Empty; // Örn: "students.create"
    public string Name { get; set; } = string.Empty; // Örn: "Öğrenci Kayıt"
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty; // Örn: "Öğrenci Yönetimi"
    public string? Icon { get; set; } // Frontend için ikon
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
