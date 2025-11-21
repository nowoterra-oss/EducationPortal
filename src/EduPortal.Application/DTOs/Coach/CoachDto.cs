namespace EduPortal.Application.DTOs.Coach;

public class CoachDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }

    public int? BranchId { get; set; }
    public string? BranchName { get; set; }

    public string Specialization { get; set; } = string.Empty;
    public string? Qualifications { get; set; }
    public int ExperienceYears { get; set; }

    public List<string> CoachingAreas { get; set; } = new List<string>(); // Human-readable names

    public bool IsAvailable { get; set; }
    public decimal? HourlyRate { get; set; }
    public string? Bio { get; set; }

    public bool IsAlsoTeacher { get; set; }
    public int? TeacherId { get; set; }
    public string? TeacherName { get; set; }

    // Statistics
    public int ActiveStudentCount { get; set; }
    public int TotalSessionsCompleted { get; set; }
    public decimal AverageRating { get; set; }

    public DateTime CreatedDate { get; set; }
}
