using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Document;

public class UpdateApplicationDocumentDto
{
    [Required]
    [MaxLength(200)]
    public string DocumentName { get; set; } = string.Empty;

    [Required]
    public int Status { get; set; } // DocumentStatus enum

    [MaxLength(500)]
    public string? DocumentUrl { get; set; }

    public DateTime? SubmissionDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
