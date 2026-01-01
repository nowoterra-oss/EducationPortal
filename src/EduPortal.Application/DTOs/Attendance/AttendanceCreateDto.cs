using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Attendance;

public class AttendanceCreateDto
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int CourseId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public AttendanceStatus Status { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [Range(0, 100)]
    public int? Performance { get; set; }

    // Ders programı bağlantısı (aynı gün birden fazla ders için)
    public int? ScheduleId { get; set; }

    [StringLength(5)]
    public string? LessonTime { get; set; } // Format: "09:00"
}
