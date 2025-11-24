namespace EduPortal.Application.DTOs.WeeklySchedule;

public class WeeklyScheduleDto
{
    public int Id { get; set; }
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public int? ClassroomId { get; set; }
    public string? ClassroomName { get; set; }
    public int AcademicTermId { get; set; }
    public string AcademicTermName { get; set; } = string.Empty;
    public DayOfWeek DayOfWeek { get; set; }
    public string DayOfWeekName => GetDayName(DayOfWeek);
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string TimeSlot => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
    public string AcademicYear { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Notes { get; set; }

    private static string GetDayName(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => "Pazartesi",
        DayOfWeek.Tuesday => "Salı",
        DayOfWeek.Wednesday => "Çarşamba",
        DayOfWeek.Thursday => "Perşembe",
        DayOfWeek.Friday => "Cuma",
        DayOfWeek.Saturday => "Cumartesi",
        DayOfWeek.Sunday => "Pazar",
        _ => day.ToString()
    };
}
