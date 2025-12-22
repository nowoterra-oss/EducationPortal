using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.SimpleInternship;

public class SimpleInternshipDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsOngoing { get; set; }
    public string? Description { get; set; }
    public string? CertificateUrl { get; set; }
    public string? CertificateFileName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateSimpleInternshipDto
{
    [Required]
    [StringLength(255)]
    public string CompanyName { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Position { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Industry { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsOngoing { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    public IFormFile? CertificateFile { get; set; }
}
