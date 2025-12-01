using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.Student;

public class StudentDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string StudentNo { get; set; } = string.Empty;
    public IdentityType IdentityType { get; set; }
    public string IdentityNumber { get; set; } = string.Empty;
    public string? Nationality { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public int CurrentGrade { get; set; }
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? Address { get; set; }
    public decimal? LGSPercentile { get; set; }
    public bool IsBilsem { get; set; }
    public string? BilsemField { get; set; }
    public string? LanguageLevel { get; set; }
    public string? TargetMajor { get; set; }
    public string? TargetCountry { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public bool IsActive { get; set; }
    public string? ProfilePhotoUrl { get; set; }
}
