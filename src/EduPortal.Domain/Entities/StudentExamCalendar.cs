using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

/// <summary>
/// Ogrenci sinav takvimi - AP, SAT, TOEFL, IELTS vb. tarihleri
/// </summary>
public class StudentExamCalendar : BaseEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ExamName { get; set; } = string.Empty; // "SAT", "AP Calculus", "TOEFL"

    [MaxLength(50)]
    public string ExamType { get; set; } = string.Empty; // "SAT", "AP", "TOEFL", "IELTS", "IB"

    public DateTime? RegistrationStartDate { get; set; }
    public DateTime? RegistrationDeadline { get; set; }
    public DateTime? ExamDate { get; set; }
    public DateTime? ResultDate { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Planned"; // "Planned", "Registered", "Completed", "Cancelled"

    [Column(TypeName = "decimal(10,2)")]
    public decimal? TargetScore { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? ActualScore { get; set; }

    public bool ReminderSent7Days { get; set; } = false;
    public bool ReminderSent1Day { get; set; } = false;

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
}
