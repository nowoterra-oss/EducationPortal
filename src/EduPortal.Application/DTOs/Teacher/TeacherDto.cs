using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.Teacher;

public class TeacherDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Specialization { get; set; }
    public int? Experience { get; set; }
    public bool IsActive { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public bool IsAlsoCounselor { get; set; }
    public int? CounselorId { get; set; }

    // Identity fields
    public IdentityType? IdentityType { get; set; }
    public string? IdentityNumber { get; set; }
    public string? Nationality { get; set; }

    // Extended fields
    public string? Department { get; set; }
    public string? Biography { get; set; }
    public string? Education { get; set; }
    public string? Certifications { get; set; }
    public string? OfficeLocation { get; set; }
    public string? OfficeHours { get; set; }
    public DateTime? HireDate { get; set; }
    public string? ProfilePhotoUrl { get; set; }

    // New fields for extended teacher form
    public int? ExperienceScore { get; set; }
    public string? CvUrl { get; set; }
    public TeacherAddressDto? Address { get; set; }
    public List<TeacherBranchDto>? Branches { get; set; }
    public List<TeacherCertificateDto>? Certificates { get; set; }
    public List<TeacherReferenceDto>? References { get; set; }
    public List<int>? WorkTypes { get; set; }

    // Maaş Bilgileri
    public decimal? MonthlySalary { get; set; }
    public decimal? HourlyRate { get; set; }
    public SalaryType SalaryType { get; set; }
}

public class TeacherAddressDto
{
    public string? Street { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
}

public class TeacherBranchDto
{
    public int? Id { get; set; }
    public int CourseId { get; set; }
    public string? CourseName { get; set; }
    public string? CourseCode { get; set; }
}

public class TeacherCertificateDto
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Institution { get; set; } = string.Empty;
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? FileUrl { get; set; }
}

public class TeacherReferenceDto
{
    public int? Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}

public class TeacherSummaryDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Specialization { get; set; }
}
