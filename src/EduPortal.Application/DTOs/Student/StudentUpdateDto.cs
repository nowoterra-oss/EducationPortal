using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Student;

public class StudentUpdateDto
{
    [Required]
    public int Id { get; set; }

    [StringLength(20)]
    public string? StudentNo { get; set; }

    [StringLength(200)]
    public string? SchoolName { get; set; }

    [Range(1, 12)]
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

    [Phone]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Öğrenci profil fotoğrafı URL'i
    /// </summary>
    [StringLength(500)]
    public string? ProfilePhotoUrl { get; set; }
}
