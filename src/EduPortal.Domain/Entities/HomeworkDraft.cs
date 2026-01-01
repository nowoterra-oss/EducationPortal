using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

/// <summary>
/// Öğretmenin hazırladığı taslak ödev (henüz öğrencilere gönderilmemiş)
/// </summary>
public class HomeworkDraft : BaseAuditableEntity
{
    [Required]
    public int TeacherId { get; set; }

    /// <summary>
    /// Frontend'deki lesson.id (unique identifier - schedule_1234 formatında)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string LessonId { get; set; } = string.Empty;

    public int? CourseId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    public DateTime? TestDueDate { get; set; }

    /// <summary>
    /// Ders sonu testi var mı?
    /// </summary>
    public bool HasTest { get; set; } = false;

    /// <summary>
    /// Öğrenci listesi JSON olarak: [{studentId, studentName, status, performance, notes}]
    /// </summary>
    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string StudentsJson { get; set; } = "[]";

    /// <summary>
    /// Ders içeriği URL'leri JSON olarak: ["url1", "url2"]
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? ContentUrlsJson { get; set; }

    /// <summary>
    /// Ders içeriği dosyaları JSON olarak: [{name, downloadUrl}]
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? ContentFilesJson { get; set; }

    /// <summary>
    /// Seçilen ders kaynakları ID'leri JSON olarak: [1, 2, 3]
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? CourseResourceIdsJson { get; set; }

    /// <summary>
    /// Test URL'leri JSON olarak: ["url1", "url2"]
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? TestUrlsJson { get; set; }

    /// <summary>
    /// Test dosyaları JSON olarak: [{name, downloadUrl}]
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? TestFilesJson { get; set; }

    /// <summary>
    /// Taslak öğrencilere gönderildi mi?
    /// </summary>
    public bool IsSent { get; set; } = false;

    /// <summary>
    /// Gönderilme tarihi
    /// </summary>
    public DateTime? SentAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(TeacherId))]
    public virtual Teacher Teacher { get; set; } = null!;

    [ForeignKey(nameof(CourseId))]
    public virtual Course? Course { get; set; }
}
