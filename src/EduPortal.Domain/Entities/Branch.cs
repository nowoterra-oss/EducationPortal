using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class Branch : BaseAuditableEntity
{
    [Required]
    [MaxLength(200)]
    public string BranchName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string BranchCode { get; set; } = string.Empty;

    [Required]
    public BranchType Type { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? District { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    public string? ManagerId { get; set; }

    public int Capacity { get; set; }

    public DateTime OpeningDate { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(ManagerId))]
    public virtual ApplicationUser? Manager { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
    public virtual ICollection<Classroom> Classrooms { get; set; } = new List<Classroom>();
    public virtual ICollection<Coach> Coaches { get; set; } = new List<Coach>();
    public virtual ICollection<CoachingSession> CoachingSessions { get; set; } = new List<CoachingSession>();
}
