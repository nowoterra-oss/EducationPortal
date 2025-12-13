namespace EduPortal.Application.DTOs.StudentGroup;

public class GroupLessonScheduleDto
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? ClassroomId { get; set; }
    public string? ClassroomName { get; set; }
    public bool IsRecurring { get; set; }
    public List<string> CancelledDates { get; set; } = new();
    public string? Notes { get; set; }
    public string? Color { get; set; }
}

public class CreateGroupLessonDto
{
    public int GroupId { get; set; }
    public int TeacherId { get; set; }
    public int CourseId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public int? ClassroomId { get; set; }
    public string? Notes { get; set; }
    public bool IsRecurring { get; set; } = true;
}

/// <summary>
/// Cakisma detayi - hangi ogrenci, hangi ogretmen, hangi saatte cakisiyor
/// </summary>
public class ConflictDetailDto
{
    public string ConflictType { get; set; } = string.Empty; // "Student" veya "Teacher"
    public int? StudentId { get; set; }
    public string? StudentName { get; set; }
    public int? TeacherId { get; set; }
    public string? TeacherName { get; set; }
    public string ConflictingCourseName { get; set; } = string.Empty;
    public string TimeRange { get; set; } = string.Empty;
    public string DateRange { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class GroupLessonConflictCheckResult
{
    public bool HasConflict { get; set; }
    public List<ConflictDetailDto> Conflicts { get; set; } = new();
}
