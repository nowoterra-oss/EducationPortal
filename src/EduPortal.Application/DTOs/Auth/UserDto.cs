namespace EduPortal.Application.DTOs.Auth;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public bool IsActive { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Kullanıcı tipi - Permission bazlı yetkilendirme için
    // Değerler: Admin, Student, Teacher, Counselor, Parent
    public string UserType { get; set; } = string.Empty;

    // Kullanıcının sahip olduğu yetkiler
    public List<string> Permissions { get; set; } = new();

    // Entity ID'ler - Frontend için gerekli
    public int? StudentId { get; set; }
    public int? TeacherId { get; set; }
    public int? ParentId { get; set; }
    public int? CounselorId { get; set; }
}
