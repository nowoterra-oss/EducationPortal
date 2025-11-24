using System.ComponentModel.DataAnnotations;
using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.UniversityApplication;

public class ApplicationStatusDto
{
    [Required(ErrorMessage = "Durum belirtilmelidir")]
    public ApplicationStatus Status { get; set; }

    [MaxLength(500, ErrorMessage = "Not en fazla 500 karakter olabilir")]
    public string? Note { get; set; }

    public DateTime? DecisionDate { get; set; }
}

public class ApplicationTimelineDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Event { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ApplicationStatus? OldStatus { get; set; }
    public ApplicationStatus? NewStatus { get; set; }
}

public class AddApplicationDocumentDto
{
    [Required(ErrorMessage = "Belge türü belirtilmelidir")]
    [MaxLength(100, ErrorMessage = "Belge türü en fazla 100 karakter olabilir")]
    public string DocumentType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Belge başlığı belirtilmelidir")]
    [MaxLength(200, ErrorMessage = "Belge başlığı en fazla 200 karakter olabilir")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Belge URL belirtilmelidir")]
    [MaxLength(500, ErrorMessage = "Belge URL en fazla 500 karakter olabilir")]
    public string DocumentUrl { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Not en fazla 500 karakter olabilir")]
    public string? Notes { get; set; }
}

public class ApplicationDocumentResultDto
{
    public int DocumentId { get; set; }
    public int ApplicationId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string DocumentUrl { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
