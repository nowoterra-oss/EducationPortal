using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Student;

public class StudentCreateDto
{
    [Required(ErrorMessage = "Ad alanı zorunludur")]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad alanı zorunludur")]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email alanı zorunludur")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Öğrenci numarası zorunludur")]
    [StringLength(20)]
    public string StudentNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Okul adı zorunludur")]
    [StringLength(200)]
    public string SchoolName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sınıf seviyesi zorunludur")]
    [Range(1, 12, ErrorMessage = "Sınıf 1-12 arasında olmalıdır")]
    public int CurrentGrade { get; set; }

    [Required(ErrorMessage = "Cinsiyet zorunludur")]
    public Gender Gender { get; set; }

    [Required(ErrorMessage = "Doğum tarihi zorunludur")]
    public DateTime DateOfBirth { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [Range(0, 100)]
    public decimal? LGSPercentile { get; set; }

    public bool IsBilsem { get; set; }

    [StringLength(100)]
    public string? BilsemField { get; set; }

    [StringLength(50)]
    public string? LanguageLevel { get; set; }

    [StringLength(200)]
    public string? TargetMajor { get; set; }

    [StringLength(100)]
    public string? TargetCountry { get; set; }

    [StringLength(200)]
    public string? ReferenceSource { get; set; }

    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
}
