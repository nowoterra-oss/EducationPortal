using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.StudentActivity;

public class StudentSummerActivityDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string ActivityName { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public string? OrganizingInstitution { get; set; }
    public string? Location { get; set; }
    public string? Country { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? DurationDays { get; set; }
    public string? Description { get; set; }
    public string? SkillsGained { get; set; }
    public string? CertificateUrl { get; set; }
    public string? DocumentUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateStudentSummerActivityDto
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    [StringLength(200)]
    public string ActivityName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string ActivityType { get; set; } = string.Empty; // "YazOkulu", "Kamp", "Workshop", "Gezi", "Diger"

    [StringLength(200)]
    public string? OrganizingInstitution { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? DurationDays { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(1000)]
    public string? SkillsGained { get; set; }

    [StringLength(500)]
    public string? CertificateUrl { get; set; }

    [StringLength(500)]
    public string? DocumentUrl { get; set; }
}

public class UpdateStudentSummerActivityDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string ActivityName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string ActivityType { get; set; } = string.Empty;

    [StringLength(200)]
    public string? OrganizingInstitution { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? DurationDays { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(1000)]
    public string? SkillsGained { get; set; }

    [StringLength(500)]
    public string? CertificateUrl { get; set; }

    [StringLength(500)]
    public string? DocumentUrl { get; set; }
}
