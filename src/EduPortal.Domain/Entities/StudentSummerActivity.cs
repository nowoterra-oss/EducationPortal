using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

/// <summary>
/// Ogrenci yaz aktiviteleri (yaz okulu, kamp, workshop, vb.)
/// </summary>
public class StudentSummerActivity : BaseEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    [MaxLength(200)]
    public string ActivityName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string ActivityType { get; set; } = string.Empty; // "YazOkulu", "Kamp", "Workshop", "Gezi", "Diger"

    [MaxLength(200)]
    public string? OrganizingInstitution { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? DurationDays { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(1000)]
    public string? SkillsGained { get; set; }

    [MaxLength(500)]
    public string? CertificateUrl { get; set; }

    [MaxLength(500)]
    public string? DocumentUrl { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
}
