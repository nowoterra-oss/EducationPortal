using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace EduPortal.Domain.Entities;

public class Course : BaseAuditableEntity
{
    [Required]
    [MaxLength(200)]
    public string CourseName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string CourseCode { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Subject { get; set; } // "Matematik", "Fizik", etc.

    [MaxLength(50)]
    public string? Level { get; set; }

    public int? Credits { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
    public virtual ICollection<StudentTeacherAssignment> StudentAssignments { get; set; } = new List<StudentTeacherAssignment>();
    public virtual ICollection<Homework> Homeworks { get; set; } = new List<Homework>();
    public virtual ICollection<Curriculum> Curriculum { get; set; } = new List<Curriculum>();
    public virtual ICollection<CourseResource> Resources { get; set; } = new List<CourseResource>();
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public virtual ICollection<ClassPerformance> ClassPerformances { get; set; } = new List<ClassPerformance>();
    public virtual ICollection<InternalExam> InternalExams { get; set; } = new List<InternalExam>();
}
