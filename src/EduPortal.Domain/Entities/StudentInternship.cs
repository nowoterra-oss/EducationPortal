using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

/// <summary>
/// Ogrenci staj bilgileri
/// </summary>
public class StudentInternship : BaseEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Department { get; set; }

    [Required]
    [MaxLength(200)]
    public string Position { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Sector { get; set; } // "Teknoloji", "Finans", "Saglik", vb.

    [MaxLength(200)]
    public string? Location { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? TotalHours { get; set; }

    public bool IsPaid { get; set; } = false;

    [MaxLength(200)]
    public string? SupervisorName { get; set; }

    [MaxLength(100)]
    public string? SupervisorTitle { get; set; }

    [MaxLength(100)]
    public string? SupervisorEmail { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(1000)]
    public string? Responsibilities { get; set; }

    [MaxLength(1000)]
    public string? SkillsGained { get; set; }

    [MaxLength(500)]
    public string? CertificateUrl { get; set; }

    [MaxLength(500)]
    public string? ReferenceLetterUrl { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
}
