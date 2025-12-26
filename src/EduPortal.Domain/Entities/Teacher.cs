using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class Teacher : BaseEntity
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Specialization { get; set; }

    public int? Experience { get; set; } // Years

    public bool IsActive { get; set; } = true;

    public int? BranchId { get; set; }

    public bool IsAlsoCounselor { get; set; } = false;

    public int? CounselorId { get; set; }

    // Identity fields
    public IdentityType? IdentityType { get; set; }

    [MaxLength(50)]
    public string? IdentityNumber { get; set; }

    [MaxLength(50)]
    public string? Nationality { get; set; }

    // Extended fields
    [MaxLength(100)]
    public string? Department { get; set; }

    [MaxLength(2000)]
    public string? Biography { get; set; }

    [MaxLength(500)]
    public string? Education { get; set; }

    [MaxLength(500)]
    public string? Certifications { get; set; }

    [MaxLength(100)]
    public string? OfficeLocation { get; set; }

    [MaxLength(100)]
    public string? OfficeHours { get; set; }

    public DateTime? HireDate { get; set; }

    [MaxLength(500)]
    public string? ProfilePhotoUrl { get; set; }

    // New fields for extended teacher form
    public int? ExperienceScore { get; set; }  // 0-100 score

    [MaxLength(500)]
    public string? CvUrl { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(BranchId))]
    public virtual Branch? Branch { get; set; }

    [ForeignKey(nameof(CounselorId))]
    public virtual Counselor? CounselorProfile { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; } = null!;

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    public virtual ICollection<Homework> Homeworks { get; set; } = new List<Homework>();
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public virtual ICollection<ClassPerformance> ClassPerformances { get; set; } = new List<ClassPerformance>();
    public virtual ICollection<StudentTeacherAssignment> StudentAssignments { get; set; } = new List<StudentTeacherAssignment>();
    public virtual ICollection<InternalExam> InternalExams { get; set; } = new List<InternalExam>();

    // New navigation properties for extended teacher form
    public virtual TeacherAddress? Address { get; set; }
    public virtual ICollection<TeacherBranch> TeacherBranches { get; set; } = new List<TeacherBranch>();
    public virtual ICollection<TeacherCertificate> TeacherCertificates { get; set; } = new List<TeacherCertificate>();
    public virtual ICollection<TeacherReference> TeacherReferences { get; set; } = new List<TeacherReference>();
    public virtual ICollection<TeacherWorkType> TeacherWorkTypes { get; set; } = new List<TeacherWorkType>();
}
