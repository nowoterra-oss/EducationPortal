using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Student;

public class HobbyDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool HasLicense { get; set; }
    public string? LicenseLevel { get; set; }
    public string? LicenseDocumentUrl { get; set; }
    public string? Achievements { get; set; }
    public DateTime? StartDate { get; set; }
}

public class HobbyCreateDto
{
    [Required(ErrorMessage = "Kategori zorunludur")]
    [MaxLength(50, ErrorMessage = "Kategori en fazla 50 karakter olabilir")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hobi adi zorunludur")]
    [MaxLength(200, ErrorMessage = "Hobi adi en fazla 200 karakter olabilir")]
    public string Name { get; set; } = string.Empty;

    public bool HasLicense { get; set; } = false;

    [MaxLength(200, ErrorMessage = "Lisans seviyesi en fazla 200 karakter olabilir")]
    public string? LicenseLevel { get; set; }

    [MaxLength(500, ErrorMessage = "Lisans dokuman URL en fazla 500 karakter olabilir")]
    public string? LicenseDocumentUrl { get; set; }

    [MaxLength(1000, ErrorMessage = "Basarilar en fazla 1000 karakter olabilir")]
    public string? Achievements { get; set; }

    public DateTime? StartDate { get; set; }
}
