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
}
