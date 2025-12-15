using System.ComponentModel.DataAnnotations;
using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.Calendar;

public class AdminCalendarEventDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CalendarEventType EventType { get; set; }
    public DateTime EventDate { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class AdminCalendarEventCreateDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public CalendarEventType EventType { get; set; }

    [Required]
    public DateTime EventDate { get; set; }

    [Required]
    public string StartTime { get; set; } = string.Empty;

    [Required]
    public string EndTime { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Location { get; set; }
}

public class AdminCalendarEventUpdateDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public CalendarEventType EventType { get; set; }

    [Required]
    public DateTime EventDate { get; set; }

    [Required]
    public string StartTime { get; set; } = string.Empty;

    [Required]
    public string EndTime { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Location { get; set; }
}
