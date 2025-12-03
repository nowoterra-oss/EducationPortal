using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Teacher;

public class TeacherUpdateDto
{
    [Required]
    public int Id { get; set; }

    [StringLength(200)]
    public string? Specialization { get; set; }

    [Range(0, 50, ErrorMessage = "Deneyim 0-50 yıl arasında olmalıdır")]
    public int? Experience { get; set; }

    public bool? IsActive { get; set; }

    public int? BranchId { get; set; }

    public bool? IsAlsoCoach { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    // Identity fields
    public IdentityType? IdentityType { get; set; }

    [StringLength(50)]
    public string? IdentityNumber { get; set; }

    [StringLength(50)]
    public string? Nationality { get; set; }

    // Extended fields
    [StringLength(100)]
    public string? Department { get; set; }

    [StringLength(2000)]
    public string? Biography { get; set; }

    [StringLength(500)]
    public string? Education { get; set; }

    [StringLength(500)]
    public string? Certifications { get; set; }

    [StringLength(100)]
    public string? OfficeLocation { get; set; }

    [StringLength(100)]
    public string? OfficeHours { get; set; }

    public DateTime? HireDate { get; set; }

    [StringLength(500)]
    public string? ProfilePhotoUrl { get; set; }
}
