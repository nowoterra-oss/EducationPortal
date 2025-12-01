using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.StudentActivity;

public class StudentInternshipDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string Position { get; set; } = string.Empty;
    public string? Sector { get; set; }
    public string? Location { get; set; }
    public string? Country { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? TotalHours { get; set; }
    public bool IsPaid { get; set; }
    public string? SupervisorName { get; set; }
    public string? SupervisorTitle { get; set; }
    public string? SupervisorEmail { get; set; }
    public string? Description { get; set; }
    public string? Responsibilities { get; set; }
    public string? SkillsGained { get; set; }
    public string? CertificateUrl { get; set; }
    public string? ReferenceLetterUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateStudentInternshipDto
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    [StringLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Department { get; set; }

    [Required]
    [StringLength(200)]
    public string Position { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Sector { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? TotalHours { get; set; }

    public bool IsPaid { get; set; } = false;

    [StringLength(200)]
    public string? SupervisorName { get; set; }

    [StringLength(100)]
    public string? SupervisorTitle { get; set; }

    [StringLength(100)]
    [EmailAddress]
    public string? SupervisorEmail { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(1000)]
    public string? Responsibilities { get; set; }

    [StringLength(1000)]
    public string? SkillsGained { get; set; }

    [StringLength(500)]
    public string? CertificateUrl { get; set; }

    [StringLength(500)]
    public string? ReferenceLetterUrl { get; set; }
}

public class UpdateStudentInternshipDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Department { get; set; }

    [Required]
    [StringLength(200)]
    public string Position { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Sector { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? TotalHours { get; set; }

    public bool IsPaid { get; set; }

    [StringLength(200)]
    public string? SupervisorName { get; set; }

    [StringLength(100)]
    public string? SupervisorTitle { get; set; }

    [StringLength(100)]
    [EmailAddress]
    public string? SupervisorEmail { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(1000)]
    public string? Responsibilities { get; set; }

    [StringLength(1000)]
    public string? SkillsGained { get; set; }

    [StringLength(500)]
    public string? CertificateUrl { get; set; }

    [StringLength(500)]
    public string? ReferenceLetterUrl { get; set; }
}
