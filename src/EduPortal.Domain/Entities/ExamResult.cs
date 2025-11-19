using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class ExamResult : BaseEntity
{
    [Required]
    public int ExamId { get; set; }

    [Required]
    public int StudentId { get; set; }

    [Required]
    [Column(TypeName = "decimal(5,2)")]
    public decimal Score { get; set; }

    [Required]
    [Range(0, 100)]
    [Column(TypeName = "decimal(5,2)")]
    public decimal Percentage { get; set; }

    public int? Rank { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [ForeignKey(nameof(ExamId))]
    public virtual InternalExam Exam { get; set; } = null!;

    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
}
