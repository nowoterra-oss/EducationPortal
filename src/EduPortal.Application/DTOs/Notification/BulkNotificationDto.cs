using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Notification;

public class BulkNotificationDto
{
    [Required(ErrorMessage = "En az bir kullanıcı belirtilmelidir")]
    [MinLength(1, ErrorMessage = "En az bir kullanıcı belirtilmelidir")]
    public List<string> UserIds { get; set; } = new();

    [Required(ErrorMessage = "Başlık boş olamaz")]
    [MaxLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mesaj boş olamaz")]
    [MaxLength(1000, ErrorMessage = "Mesaj en fazla 1000 karakter olabilir")]
    public string Message { get; set; } = string.Empty;

    public NotificationType Type { get; set; } = NotificationType.Info;

    [MaxLength(500)]
    public string? ActionUrl { get; set; }

    [MaxLength(100)]
    public string? ActionText { get; set; }
}
