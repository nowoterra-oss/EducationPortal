namespace EduPortal.Application.DTOs.CounselorDashboard;

public class StudentFullProfileDto
{
    // Temel Bilgiler
    public int Id { get; set; }
    public string StudentNo { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public int CurrentGrade { get; set; }
    public string? SchoolName { get; set; }
    public DateTime DateOfBirth { get; set; }

    // LGS ve Giris Sinavi
    public decimal? LGSPercentile { get; set; }
    public List<ReadinessExamDto> EntranceExams { get; set; } = new();

    // Veli Bilgileri
    public List<ParentInfoDto> Parents { get; set; } = new();

    // Hedefler
    public string? TargetMajor { get; set; }
    public string? TargetCountry { get; set; }
    public bool IsBilsem { get; set; }
    public string? BilsemField { get; set; }
    public string? LanguageLevel { get; set; }

    // Ders Veren Ogretmenler
    public List<AssignedTeacherDto> AssignedTeachers { get; set; } = new();

    // Haftalik Program
    public List<WeeklyScheduleItemDto> WeeklySchedule { get; set; } = new();
}

public class ParentInfoDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Occupation { get; set; }
    public string Relationship { get; set; } = string.Empty; // "Anne", "Baba"
}

public class AssignedTeacherDto
{
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? CourseName { get; set; }
}

public class WeeklyScheduleItemDto
{
    public int DayOfWeek { get; set; }
    public string DayName { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
}

public class ReadinessExamDto
{
    public int Id { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public DateTime ExamDate { get; set; }
    public decimal? Score { get; set; }
    public decimal? MaxScore { get; set; }
    public decimal? Percentage { get; set; }
    public string? Analysis { get; set; }
}
