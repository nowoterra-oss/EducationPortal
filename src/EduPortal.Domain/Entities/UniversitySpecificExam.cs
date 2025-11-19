using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class UniversitySpecificExam : BaseEntity
{
    [Required]
    public int ApplicationId { get; set; }

    [Required]
    [MaxLength(200)]
    public string ExamName { get; set; } = string.Empty;

    public DateTime? ExamDate { get; set; }

    [MaxLength(50)]
    public string? Score { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty; // "Planlanıyor", "Tamamlandi"

    [ForeignKey(nameof(ApplicationId))]
    public virtual UniversityApplication Application { get; set; } = null!;
}
