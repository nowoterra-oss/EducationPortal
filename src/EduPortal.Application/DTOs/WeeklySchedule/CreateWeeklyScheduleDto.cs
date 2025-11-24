using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.WeeklySchedule;

public class CreateWeeklyScheduleDto
{
    [Required(ErrorMessage = "Sınıf belirtilmelidir")]
    public int ClassId { get; set; }

    [Required(ErrorMessage = "Ders belirtilmelidir")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Öğretmen belirtilmelidir")]
    public int TeacherId { get; set; }

    public int? ClassroomId { get; set; }

    [Required(ErrorMessage = "Akademik dönem belirtilmelidir")]
    public int AcademicTermId { get; set; }

    [Required(ErrorMessage = "Gün belirtilmelidir")]
    public DayOfWeek DayOfWeek { get; set; }

    [Required(ErrorMessage = "Başlangıç saati belirtilmelidir")]
    public TimeSpan StartTime { get; set; }

    [Required(ErrorMessage = "Bitiş saati belirtilmelidir")]
    public TimeSpan EndTime { get; set; }

    [Required(ErrorMessage = "Akademik yıl belirtilmelidir")]
    [MaxLength(20, ErrorMessage = "Akademik yıl en fazla 20 karakter olabilir")]
    public string AcademicYear { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    [MaxLength(500, ErrorMessage = "Notlar en fazla 500 karakter olabilir")]
    public string? Notes { get; set; }
}
