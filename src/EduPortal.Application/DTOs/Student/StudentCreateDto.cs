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

    // StudentNo backend tarafından otomatik oluşturulacak

    /// <summary>
    /// Kimlik belgesi türü (0: TC Kimlik, 1: Pasaport, 2: Yabancı Kimlik, 3: Diğer)
    /// </summary>
    [Required(ErrorMessage = "Kimlik türü zorunludur")]
    public IdentityType IdentityType { get; set; } = IdentityType.TCKimlik;

    /// <summary>
    /// Kimlik numarası (TC Kimlik No, Pasaport No, vb.)
    /// </summary>
    [Required(ErrorMessage = "Kimlik numarası zorunludur")]
    [StringLength(50)]
    public string IdentityNumber { get; set; } = string.Empty;

    /// <summary>
    /// Uyruk/Vatandaşlık (varsayılan: TR)
    /// </summary>
    [StringLength(100)]
    public string? Nationality { get; set; } = "TR";

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

    /// <summary>
    /// Mülakat sonucu
    /// </summary>
    public InterviewResult? InterviewResult { get; set; }

    /// <summary>
    /// Ogrenci gorusmeleri (JSON formatinda)
    /// </summary>
    public string? InterviewsJson { get; set; }

    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Ogrenci profil fotografı URL'i (opsiyonel, kayit oncesi yuklenen)
    /// </summary>
    [StringLength(500)]
    public string? ProfilePhotoUrl { get; set; }
}
