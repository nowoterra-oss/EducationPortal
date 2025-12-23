namespace EduPortal.Application.DTOs.Permission;

/// <summary>
/// Toplu yetki güncelleme için DTO
/// </summary>
public class BulkPermissionUpdateDto
{
    /// <summary>
    /// Verilecek yetki ID'leri
    /// </summary>
    public List<int> GrantedPermissionIds { get; set; } = new();

    /// <summary>
    /// Engellenecek yetki ID'leri (role yetkilerini override eder)
    /// </summary>
    public List<int> DeniedPermissionIds { get; set; } = new();

    /// <summary>
    /// Açıklama/notlar
    /// </summary>
    public string? Notes { get; set; }
}
