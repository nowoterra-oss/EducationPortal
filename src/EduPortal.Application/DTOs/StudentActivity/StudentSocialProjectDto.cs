using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.StudentActivity;

public class StudentSocialProjectDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectType { get; set; } = string.Empty;
    public string? OrganizationName { get; set; }
    public string? Category { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? TotalHours { get; set; }
    public int? ImpactedPeopleCount { get; set; }
    public string? Description { get; set; }
    public string? Objectives { get; set; }
    public string? Outcomes { get; set; }
    public string? SkillsGained { get; set; }
    public string? CertificateUrl { get; set; }
    public string? DocumentUrl { get; set; }
    public string? MediaUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateStudentSocialProjectDto
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    [StringLength(200)]
    public string ProjectName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string ProjectType { get; set; } = string.Empty; // "Gonulluluk", "BagisKampanyasi", "CevreProjesi", "EgitimProjesi", "Diger"

    [StringLength(200)]
    public string? OrganizationName { get; set; }

    [StringLength(100)]
    public string? Category { get; set; }

    [Required]
    [StringLength(100)]
    public string Role { get; set; } = string.Empty; // "Katilimci", "Koordinator", "Lider", "Kurucu"

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? TotalHours { get; set; }

    public int? ImpactedPeopleCount { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(1000)]
    public string? Objectives { get; set; }

    [StringLength(1000)]
    public string? Outcomes { get; set; }

    [StringLength(1000)]
    public string? SkillsGained { get; set; }

    [StringLength(500)]
    public string? CertificateUrl { get; set; }

    [StringLength(500)]
    public string? DocumentUrl { get; set; }

    [StringLength(500)]
    public string? MediaUrl { get; set; }
}

public class UpdateStudentSocialProjectDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string ProjectName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string ProjectType { get; set; } = string.Empty;

    [StringLength(200)]
    public string? OrganizationName { get; set; }

    [StringLength(100)]
    public string? Category { get; set; }

    [Required]
    [StringLength(100)]
    public string Role { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? TotalHours { get; set; }

    public int? ImpactedPeopleCount { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(1000)]
    public string? Objectives { get; set; }

    [StringLength(1000)]
    public string? Outcomes { get; set; }

    [StringLength(1000)]
    public string? SkillsGained { get; set; }

    [StringLength(500)]
    public string? CertificateUrl { get; set; }

    [StringLength(500)]
    public string? DocumentUrl { get; set; }

    [StringLength(500)]
    public string? MediaUrl { get; set; }
}
