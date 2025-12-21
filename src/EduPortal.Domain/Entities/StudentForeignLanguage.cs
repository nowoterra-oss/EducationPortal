using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

/// <summary>
/// Ogrencinin yabanci dil bilgisi
/// </summary>
public class StudentForeignLanguage : BaseEntity
{
    [Required]
    public int StudentId { get; set; }

    /// <summary>
    /// Dil adi (Ingilizce, Almanca, Fransizca vb.)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// Dil seviyesi (A1, A2, B1, B2, C1, C2)
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string Level { get; set; } = string.Empty;

    // Navigation Properties
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
}
