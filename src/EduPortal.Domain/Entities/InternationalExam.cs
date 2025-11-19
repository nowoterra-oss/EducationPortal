using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class InternationalExam : BaseEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public ExamType ExamType { get; set; }

    [Required]
    [MaxLength(200)]
    public string ExamName { get; set; } = string.Empty;

    public int? Grade { get; set; }

    [Required]
    [MaxLength(20)]
    public string AcademicYear { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Score { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? MaxScore { get; set; }

    public DateTime? ApplicationStartDate { get; set; }

    public DateTime? ApplicationEndDate { get; set; }

    [Required]
    public DateTime ExamDate { get; set; }

    public DateTime? ResultDate { get; set; }

    [MaxLength(500)]
    public string? CertificateUrl { get; set; }

    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
}
