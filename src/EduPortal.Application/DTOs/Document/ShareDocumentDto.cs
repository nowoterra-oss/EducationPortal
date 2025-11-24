using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Document;

public class ShareDocumentDto
{
    [Required(ErrorMessage = "Kullanıcı ID belirtilmelidir")]
    public int UserId { get; set; }

    [MaxLength(500, ErrorMessage = "Mesaj en fazla 500 karakter olabilir")]
    public string? Message { get; set; }

    public bool CanEdit { get; set; } = false;

    public DateTime? ExpiresAt { get; set; }
}

public class DocumentShareResultDto
{
    public int DocumentId { get; set; }
    public int SharedWithUserId { get; set; }
    public string SharedWithUserName { get; set; } = string.Empty;
    public string ShareUrl { get; set; } = string.Empty;
    public DateTime SharedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool CanEdit { get; set; }
}
