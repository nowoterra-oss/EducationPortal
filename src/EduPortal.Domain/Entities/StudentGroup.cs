using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace EduPortal.Domain.Entities;

/// <summary>
/// Öğrenci Grubu - Birden fazla öğrencinin oluşturduğu grup
/// </summary>
public class StudentGroup : BaseAuditableEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Grup tipi: "Matematik Grubu", "İngilizce A1", vb.
    /// </summary>
    [MaxLength(50)]
    public string? GroupType { get; set; }

    /// <summary>
    /// Maksimum öğrenci sayısı
    /// </summary>
    public int? MaxCapacity { get; set; }

    /// <summary>
    /// Grup rengi (takvimde gösterim için)
    /// </summary>
    [MaxLength(20)]
    public string? Color { get; set; } = "#6366F1";

    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual ICollection<StudentGroupMember> Members { get; set; } = new List<StudentGroupMember>();
    public virtual ICollection<GroupLessonSchedule> LessonSchedules { get; set; } = new List<GroupLessonSchedule>();
}
