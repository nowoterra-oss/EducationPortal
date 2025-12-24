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

    // New fields for extended teacher form
    [Range(0, 100, ErrorMessage = "Deneyim puanı 0-100 arasında olmalıdır")]
    public int? ExperienceScore { get; set; }

    [StringLength(500)]
    public string? CvUrl { get; set; }

    public TeacherAddressDto? Address { get; set; }
    public List<TeacherBranchDto>? Branches { get; set; }
    public List<TeacherCertificateDto>? Certificates { get; set; }
    public List<TeacherReferenceDto>? References { get; set; }
    public List<int>? WorkTypes { get; set; }

    /// <summary>
    /// Danışman olarak atanacak öğrenci ID'leri
    /// </summary>
    public List<int>? AdvisorStudentIds { get; set; }

    /// <summary>
    /// Koç olarak atanacak öğrenci ID'leri
    /// </summary>
    public List<int>? CoachStudentIds { get; set; }
}
