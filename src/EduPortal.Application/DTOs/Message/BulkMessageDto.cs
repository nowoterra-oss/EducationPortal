using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Message;

public class BulkMessageDto
{
    [Required(ErrorMessage = "En az bir alıcı belirtilmelidir")]
    [MinLength(1, ErrorMessage = "En az bir alıcı belirtilmelidir")]
    public List<string> RecipientIds { get; set; } = new();

    [MaxLength(200, ErrorMessage = "Konu en fazla 200 karakter olabilir")]
    public string? Subject { get; set; }

    [Required(ErrorMessage = "Mesaj içeriği boş olamaz")]
    [MaxLength(5000, ErrorMessage = "Mesaj en fazla 5000 karakter olabilir")]
    public string Body { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? AttachmentUrl { get; set; }
}
