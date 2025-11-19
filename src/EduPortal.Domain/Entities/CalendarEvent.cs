using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class CalendarEvent : BaseAuditableEntity
{
    public int? StudentId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    public EventType EventType { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool AllDayEvent { get; set; } = false;

    [MaxLength(200)]
    public string? Location { get; set; }

    public bool IsCompleted { get; set; } = false;

    [Required]
    public Priority Priority { get; set; }

    public DateTime? Reminder { get; set; }

    [ForeignKey(nameof(StudentId))]
    public virtual Student? Student { get; set; }
}
