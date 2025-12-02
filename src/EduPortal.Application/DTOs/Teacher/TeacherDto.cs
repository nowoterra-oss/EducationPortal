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
    public bool IsAlsoCoach { get; set; }
    public int? CoachId { get; set; }

    // Extended fields
    public string? Department { get; set; }
    public string? Biography { get; set; }
    public string? Education { get; set; }
    public string? Certifications { get; set; }
    public string? OfficeLocation { get; set; }
    public string? OfficeHours { get; set; }
    public DateTime? HireDate { get; set; }
}
