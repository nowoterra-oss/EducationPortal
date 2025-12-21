using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

/// <summary>
/// Ogrencinin aktivite bilgisi
/// </summary>
public class StudentActivity : BaseEntity
{
    [Required]
    public int StudentId { get; set; }

    /// <summary>
    /// Aktivite adi
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Aktivite turu (Spor, Kultur, Sanat vb.)
    /// </summary>
    [MaxLength(100)]
    public string? Type { get; set; }

    /// <summary>
    /// Organizasyon adi
    /// </summary>
    [MaxLength(200)]
    public string? Organization { get; set; }

    /// <summary>
    /// Aciklama
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Baslangic tarihi
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Bitis tarihi
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Devam ediyor mu?
    /// </summary>
    public bool IsOngoing { get; set; } = false;

    /// <summary>
    /// Kazanimlar/Basarilar
    /// </summary>
    [MaxLength(1000)]
    public string? Achievements { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
}
