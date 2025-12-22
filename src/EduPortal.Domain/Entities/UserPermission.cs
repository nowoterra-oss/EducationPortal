using EduPortal.Domain.Common;

namespace EduPortal.Domain.Entities;

public class UserPermission : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public int PermissionId { get; set; }
    public bool IsGranted { get; set; } = true; // true: izin ver, false: yasakla
    public DateTime? ExpiresAt { get; set; } // Opsiyonel: Geçici yetki
    public string? GrantedByUserId { get; set; } // Yetkiyi veren admin
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    // Navigation
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
    public virtual ApplicationUser? GrantedByUser { get; set; }
}
