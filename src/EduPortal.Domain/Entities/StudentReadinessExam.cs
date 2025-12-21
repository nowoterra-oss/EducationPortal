using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

/// <summary>
/// Ogrencinin hazir bulunusluk sinavi bilgisi
/// </summary>
public class StudentReadinessExam : BaseEntity
{
    [Required]
    public int StudentId { get; set; }

    /// <summary>
    /// Sinav adi
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string ExamName { get; set; } = string.Empty;

    /// <summary>
    /// Sinav tarihi
    /// </summary>
    public DateTime? ExamDate { get; set; }

    /// <summary>
    /// Sinav puani/sonucu
    /// </summary>
    [MaxLength(50)]
    public string? Score { get; set; }

    /// <summary>
    /// Notlar
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
}
