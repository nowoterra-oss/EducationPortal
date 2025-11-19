using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class Student : BaseAuditableEntity
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string StudentNo { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string SchoolName { get; set; } = string.Empty;

    [Required]
    [Range(1, 12)]
    public int CurrentGrade { get; set; }

    [Required]
    public Gender Gender { get; set; }

    [Required]
    public DateTime DateOfBirth { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [Range(0, 100)]
    [Column(TypeName = "decimal(5,2)")]
    public decimal? LGSPercentile { get; set; }

    public bool IsBilsem { get; set; } = false;

    [MaxLength(200)]
    public string? BilsemField { get; set; }

    [MaxLength(10)]
    public string? LanguageLevel { get; set; }

    [MaxLength(200)]
    public string? TargetMajor { get; set; }

    [MaxLength(100)]
    public string? TargetCountry { get; set; }

    [MaxLength(200)]
    public string? ReferenceSource { get; set; }

    [Required]
    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; } = null!;

    public virtual ICollection<Parent> Parents { get; set; } = new List<Parent>();
    public virtual ICollection<StudentSibling> Siblings { get; set; } = new List<StudentSibling>();
    public virtual ICollection<StudentHobby> Hobbies { get; set; } = new List<StudentHobby>();
    public virtual ICollection<StudentClubMembership> Clubs { get; set; } = new List<StudentClubMembership>();
    public virtual ICollection<InternationalExam> InternationalExams { get; set; } = new List<InternationalExam>();
    public virtual ICollection<CompetitionAndAward> Competitions { get; set; } = new List<CompetitionAndAward>();
    public virtual ICollection<StudentDocument> Documents { get; set; } = new List<StudentDocument>();
    public virtual ICollection<StudentHomeworkSubmission> Homeworks { get; set; } = new List<StudentHomeworkSubmission>();
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public virtual ICollection<ClassPerformance> Performances { get; set; } = new List<ClassPerformance>();
    public virtual ICollection<StudentTeacherAssignment> TeacherAssignments { get; set; } = new List<StudentTeacherAssignment>();
    public virtual ICollection<StudentCounselorAssignment> CounselorAssignments { get; set; } = new List<StudentCounselorAssignment>();
    public virtual ICollection<CounselingMeeting> CounselingMeetings { get; set; } = new List<CounselingMeeting>();
    public virtual ICollection<AcademicDevelopmentPlan> AcademicDevelopmentPlans { get; set; } = new List<AcademicDevelopmentPlan>();
    public virtual ICollection<UniversityApplication> UniversityApplications { get; set; } = new List<UniversityApplication>();
    public virtual ICollection<CalendarEvent> CalendarEvents { get; set; } = new List<CalendarEvent>();
    public virtual ICollection<PaymentPlan> PaymentPlans { get; set; } = new List<PaymentPlan>();
    public virtual ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
}
