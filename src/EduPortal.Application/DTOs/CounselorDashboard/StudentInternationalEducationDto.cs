namespace EduPortal.Application.DTOs.CounselorDashboard;

public class StudentInternationalEducationDto
{
    public int StudentId { get; set; }

    // Yurtdisi Sinavlar (AP, SAT, TOEFL, IELTS, IB)
    public List<InternationalExamDto> InternationalExams { get; set; } = new();

    // Yurtdisi Dersler ve Puanlar (Yila gore gruplu)
    public List<AcademicYearCoursesDto> CoursesByYear { get; set; } = new();

    // Sinav Takvimi
    public List<ExamCalendarItemDto> ExamCalendar { get; set; } = new();
}

public class InternationalExamDto
{
    public int Id { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public string ExamType { get; set; } = string.Empty; // "AP", "SAT", "TOEFL", "IELTS", "IB"
    public DateTime? RegistrationDeadline { get; set; }
    public DateTime? ExamDate { get; set; }
    public decimal? Score { get; set; }
    public decimal? MaxScore { get; set; }
    public string? Status { get; set; }
}

public class AcademicYearCoursesDto
{
    public string AcademicYear { get; set; } = string.Empty; // "2024-2025"
    public List<InternationalCourseDto> Courses { get; set; } = new();
}

public class InternationalCourseDto
{
    public string CourseName { get; set; } = string.Empty; // "AP Calculus AB"
    public string CourseType { get; set; } = string.Empty; // "AP", "IB"
    public decimal? Score { get; set; }
    public string? Grade { get; set; }
}

public class ExamCalendarItemDto
{
    public int Id { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public string ExamType { get; set; } = string.Empty;
    public DateTime? RegistrationDeadline { get; set; }
    public DateTime? ExamDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int DaysUntilExam { get; set; }
    public int DaysUntilDeadline { get; set; }
    public bool IsUrgent { get; set; } // 7 gun veya daha az
}
