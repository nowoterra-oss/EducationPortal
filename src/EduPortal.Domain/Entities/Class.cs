using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class Class : BaseAuditableEntity
{
    [Required]
    [MaxLength(50)]
    public string ClassName { get; set; } = string.Empty; // "9-A", "10-B"

    [Required]
    public int Grade { get; set; } // 9, 10, 11, 12

    [Required]
    [MaxLength(10)]
    public string Branch { get; set; } = string.Empty; // "A", "B", "C"

    public int? ClassTeacherId { get; set; }

    [Required]
    public int Capacity { get; set; }

    [Required]
    [MaxLength(20)]
    public string AcademicYear { get; set; } = string.Empty; // "2024-2025"

    public int? BranchId { get; set; } // Kampüs/Şube

    public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(ClassTeacherId))]
    public virtual Teacher? ClassTeacher { get; set; }

    [ForeignKey(nameof(BranchId))]
    public virtual Branch? BranchLocation { get; set; }

    public virtual ICollection<StudentClassAssignment> Students { get; set; } = new List<StudentClassAssignment>();
    public virtual ICollection<WeeklySchedule> Schedules { get; set; } = new List<WeeklySchedule>();
    public virtual ICollection<Homework> Homeworks { get; set; } = new List<Homework>();
    public virtual ICollection<InternalExam> Exams { get; set; } = new List<InternalExam>();
}
