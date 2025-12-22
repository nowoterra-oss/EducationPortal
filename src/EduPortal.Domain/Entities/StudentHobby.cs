using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class StudentHobby : BaseEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty; // "Spor", "Sanat", "Diger"

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public bool HasLicense { get; set; } = false;

    [MaxLength(200)]
    public string? LicenseLevel { get; set; }

    [MaxLength(500)]
    public string? LicenseDocumentUrl { get; set; }

    [MaxLength(1000)]
    public string? Achievements { get; set; }

    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Deneyim yili
    /// </summary>
    public int? YearsOfExperience { get; set; }

    /// <summary>
    /// Hobi aciklamasi
    /// </summary>
    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// Sertifika URL
    /// </summary>
    [MaxLength(500)]
    public string? CertificateUrl { get; set; }

    /// <summary>
    /// Sertifika dosya adi
    /// </summary>
    [MaxLength(255)]
    public string? CertificateFileName { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
}
