using System.ComponentModel.DataAnnotations;
using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.Schedule;

public class ScheduleDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public DayOfWeek DayOfWeek { get; set; }
    public string DayName { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsRecurring { get; set; }
    public LessonStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int? ClassroomId { get; set; }
    public string? ClassroomName { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateScheduleDto
{
    [Required(ErrorMessage = "Öğrenci belirtilmelidir")]
    public int StudentId { get; set; }

    [Required(ErrorMessage = "Öğretmen belirtilmelidir")]
    public int TeacherId { get; set; }

    [Required(ErrorMessage = "Ders belirtilmelidir")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Gün belirtilmelidir")]
    public DayOfWeek DayOfWeek { get; set; }

    [Required(ErrorMessage = "Başlangıç saati belirtilmelidir")]
    public TimeSpan StartTime { get; set; }

    [Required(ErrorMessage = "Bitiş saati belirtilmelidir")]
    public TimeSpan EndTime { get; set; }

    [Required(ErrorMessage = "Geçerlilik başlangıç tarihi belirtilmelidir")]
    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public bool IsRecurring { get; set; } = true;

    public int? ClassroomId { get; set; }

    [MaxLength(500, ErrorMessage = "Notlar en fazla 500 karakter olabilir")]
    public string? Notes { get; set; }
}

public class UpdateScheduleDto
{
    [Required(ErrorMessage = "Öğretmen belirtilmelidir")]
    public int TeacherId { get; set; }

    [Required(ErrorMessage = "Ders belirtilmelidir")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Gün belirtilmelidir")]
    public DayOfWeek DayOfWeek { get; set; }

    [Required(ErrorMessage = "Başlangıç saati belirtilmelidir")]
    public TimeSpan StartTime { get; set; }

    [Required(ErrorMessage = "Bitiş saati belirtilmelidir")]
    public TimeSpan EndTime { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public bool IsRecurring { get; set; }

    public LessonStatus Status { get; set; }

    public int? ClassroomId { get; set; }

    [MaxLength(500, ErrorMessage = "Notlar en fazla 500 karakter olabilir")]
    public string? Notes { get; set; }
}
