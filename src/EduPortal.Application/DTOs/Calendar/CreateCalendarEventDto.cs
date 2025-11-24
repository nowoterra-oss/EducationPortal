using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Calendar;

public class CreateCalendarEventDto
{
    public int? StudentId { get; set; }
    public int? ClassId { get; set; }

    [Required(ErrorMessage = "Etkinlik kapsamı belirtilmelidir")]
    public EventScope Scope { get; set; } = EventScope.Personal;

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

    public bool AllDayEvent { get; set; } = false;

    [MaxLength(200, ErrorMessage = "Konum en fazla 200 karakter olabilir")]
    public string? Location { get; set; }

    [Required(ErrorMessage = "Öncelik belirtilmelidir")]
    public Priority Priority { get; set; } = Priority.Normal;

    public DateTime? Reminder { get; set; }
}
