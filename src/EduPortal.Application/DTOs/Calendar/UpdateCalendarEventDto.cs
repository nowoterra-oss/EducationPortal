using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Calendar;

public class UpdateCalendarEventDto
{
    public int? StudentId { get; set; }
    public int? ClassId { get; set; }

    [Required(ErrorMessage = "Etkinlik kapsamı belirtilmelidir")]
    public EventScope Scope { get; set; }

    [Required(ErrorMessage = "Başlık boş olamaz")]
    [MaxLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Açıklama en fazla 2000 karakter olabilir")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Etkinlik tipi belirtilmelidir")]
    public EventType EventType { get; set; }

    [Required(ErrorMessage = "Başlangıç tarihi belirtilmelidir")]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool AllDayEvent { get; set; }

    [MaxLength(200, ErrorMessage = "Konum en fazla 200 karakter olabilir")]
    public string? Location { get; set; }

    public bool IsCompleted { get; set; }

    [Required(ErrorMessage = "Öncelik belirtilmelidir")]
    public Priority Priority { get; set; }

    public DateTime? Reminder { get; set; }
}
