using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

/// <summary>
/// Ogrenci odulleri - Uluslararasi, Ulusal, Yerel kategorilerinde
/// </summary>
public class StudentAward : BaseEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    [MaxLength(200)]
    public string AwardName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Scope { get; set; } = string.Empty; // "International", "National", "Local"

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty; // "Scientific", "Artistic", "Athletic"

    [MaxLength(200)]
    public string? IssuingOrganization { get; set; }

    public DateTime? AwardDate { get; set; }

    [MaxLength(100)]
    public string? Rank { get; set; } // "1st Place", "Gold Medal", "Honorable Mention"

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? CertificateUrl { get; set; }

    // Navigation
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
}
