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
    /// Aktivite kategorisi (Spor, Kultur, Sanat vb.)
    /// </summary>
    [MaxLength(100)]
    public string? Category { get; set; }

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

    // Navigation Properties
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
}
