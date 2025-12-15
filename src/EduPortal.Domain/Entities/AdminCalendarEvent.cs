using System.ComponentModel.DataAnnotations;
using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;

namespace EduPortal.Domain.Entities;

public class AdminCalendarEvent : BaseAuditableEntity
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
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }
}
