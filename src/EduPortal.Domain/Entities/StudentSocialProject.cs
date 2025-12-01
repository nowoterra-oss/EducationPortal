using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

/// <summary>
/// Ogrenci sosyal sorumluluk projeleri ve gonulluluk faaliyetleri
/// </summary>
public class StudentSocialProject : BaseEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    [MaxLength(200)]
    public string ProjectName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string ProjectType { get; set; } = string.Empty; // "Gonulluluk", "BagisKampanyasi", "CevreProjesi", "EgitimProjesi", "Diger"

    [MaxLength(200)]
    public string? OrganizationName { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; } // "Egitim", "Cevre", "Saglik", "Hayvan", "Yasli", "Cocuk", vb.

    [Required]
    [MaxLength(100)]
    public string Role { get; set; } = string.Empty; // "Katilimci", "Koordinator", "Lider", "Kurucu"

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? TotalHours { get; set; }

    public int? ImpactedPeopleCount { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(1000)]
    public string? Objectives { get; set; }

    [MaxLength(1000)]
    public string? Outcomes { get; set; }

    [MaxLength(1000)]
    public string? SkillsGained { get; set; }

    [MaxLength(500)]
    public string? CertificateUrl { get; set; }

    [MaxLength(500)]
    public string? DocumentUrl { get; set; }

    [MaxLength(500)]
    public string? MediaUrl { get; set; } // Proje fotograflari/videolari

    // Navigation Properties
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
}
