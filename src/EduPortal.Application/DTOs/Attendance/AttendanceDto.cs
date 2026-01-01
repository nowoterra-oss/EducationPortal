using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.Attendance;

public class AttendanceDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Notes { get; set; }
    public int? Performance { get; set; }
    public DateTime CreatedAt { get; set; }

    // Ders programı bağlantısı (aynı gün birden fazla ders için)
    public int? ScheduleId { get; set; }
    public string? LessonTime { get; set; } // Format: "09:00"
}

/// <summary>
/// Bugünkü yoklama özeti - ders bazında
/// </summary>
public class TodayAttendanceSummaryDto
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public List<StudentAttendanceSummaryDto> Students { get; set; } = new();
    public HomeworkSummaryDto? Homework { get; set; }
}

/// <summary>
/// Öğrenci yoklama özet bilgisi
/// </summary>
public class StudentAttendanceSummaryDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public AttendanceStatus Status { get; set; }
    public int Performance { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Ödev özet bilgisi
/// </summary>
public class HomeworkSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int StudentCount { get; set; }
}
