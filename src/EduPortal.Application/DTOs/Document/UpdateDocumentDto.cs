using System.ComponentModel.DataAnnotations;
using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.Document;

public class UpdateDocumentDto
{
    [Required(ErrorMessage = "Belge türü belirtilmelidir")]
    public DocumentType DocumentType { get; set; }

    [Required(ErrorMessage = "Başlık belirtilmelidir")]
    [MaxLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Akademik yıl belirtilmelidir")]
    [MaxLength(20, ErrorMessage = "Akademik yıl en fazla 20 karakter olabilir")]
    public string AcademicYear { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Belge URL en fazla 500 karakter olabilir")]
    public string? DocumentUrl { get; set; }

    [Required(ErrorMessage = "Durum belirtilmelidir")]
    public DocumentStatus Status { get; set; }

    [MaxLength(2000, ErrorMessage = "İnceleme notları en fazla 2000 karakter olabilir")]
    public string? ReviewNotes { get; set; }
}
