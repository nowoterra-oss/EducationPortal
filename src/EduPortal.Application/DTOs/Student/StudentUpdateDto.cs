using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Student;

public class StudentUpdateDto
{
    [Required]
    public int Id { get; set; }

    [StringLength(20)]
    public string? StudentNo { get; set; }

    // KullanÄ±cÄ± bilgileri (ApplicationUser'da gÃ¼ncellenir)
    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    [EmailAddress]
    [StringLength(200)]
    public string? Email { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    // Kimlik bilgileri
    public IdentityType? IdentityType { get; set; }

    [StringLength(50)]
    public string? IdentityNumber { get; set; }

    [StringLength(100)]
    public string? Nationality { get; set; }

    // Okul ve akademik bilgiler
    [StringLength(200)]
    public string? SchoolName { get; set; }

    [Range(1, 14)]
    public int? CurrentGrade { get; set; }

    public Gender? Gender { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [Range(0, 100)]
    public decimal? LGSPercentile { get; set; }

    public bool? IsBilsem { get; set; }

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
    /// MÃ¼lakat sonucu
    /// </summary>
    public InterviewResult? InterviewResult { get; set; }

    /// <summary>
    /// Öðrenci görüþmeleri (JSON formatýnda)
    /// </summary>
    public string? InterviewsJson { get; set; }

    public DateTime? EnrollmentDate { get; set; }

    /// <summary>
    /// Ã–ÄŸrenci profil fotoÄŸrafÄ± URL'i
    /// </summary>
    [StringLength(500)]
    public string? ProfilePhotoUrl { get; set; }

    /// <summary>
    /// Ã–ÄŸrenci aktif/pasif durumu
    /// </summary>
    public bool? IsActive { get; set; }
}
