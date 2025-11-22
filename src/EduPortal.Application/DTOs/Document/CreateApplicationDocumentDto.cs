using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Document;

public class CreateApplicationDocumentDto
{
    [Required]
    public int ProgramId { get; set; }

    [Required]
    [MaxLength(200)]
    public string DocumentName { get; set; } = string.Empty;

    [Required]
    public int DocumentType { get; set; } // DocumentType enum

    [Required]
    [MaxLength(500)]
    public string DocumentUrl { get; set; } = string.Empty;

    public DateTime? SubmissionDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
