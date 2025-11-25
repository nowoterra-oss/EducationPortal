using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Hobby;

public class HobbyDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool HasLicense { get; set; }
    public string? LicenseLevel { get; set; }
    public string? LicenseDocumentUrl { get; set; }
    public string? Achievements { get; set; }
    public DateTime? StartDate { get; set; }
}

public class CreateHobbyDto
{
    [Required(ErrorMessage = "Öğrenci belirtilmelidir")]
    public int StudentId { get; set; }

    [Required(ErrorMessage = "Kategori belirtilmelidir")]
    [MaxLength(50, ErrorMessage = "Kategori en fazla 50 karakter olabilir")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hobi adı belirtilmelidir")]
    [MaxLength(200, ErrorMessage = "Hobi adı en fazla 200 karakter olabilir")]
    public string Name { get; set; } = string.Empty;

    public bool HasLicense { get; set; } = false;

    [MaxLength(200, ErrorMessage = "Lisans seviyesi en fazla 200 karakter olabilir")]
    public string? LicenseLevel { get; set; }

    [MaxLength(500, ErrorMessage = "Lisans belgesi URL en fazla 500 karakter olabilir")]
    public string? LicenseDocumentUrl { get; set; }

    [MaxLength(1000, ErrorMessage = "Başarılar en fazla 1000 karakter olabilir")]
    public string? Achievements { get; set; }

    public DateTime? StartDate { get; set; }
}

public class UpdateHobbyDto
{
    [Required(ErrorMessage = "Kategori belirtilmelidir")]
    [MaxLength(50, ErrorMessage = "Kategori en fazla 50 karakter olabilir")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hobi adı belirtilmelidir")]
    [MaxLength(200, ErrorMessage = "Hobi adı en fazla 200 karakter olabilir")]
    public string Name { get; set; } = string.Empty;

    public bool HasLicense { get; set; }

    [MaxLength(200, ErrorMessage = "Lisans seviyesi en fazla 200 karakter olabilir")]
    public string? LicenseLevel { get; set; }

    [MaxLength(500, ErrorMessage = "Lisans belgesi URL en fazla 500 karakter olabilir")]
    public string? LicenseDocumentUrl { get; set; }

    [MaxLength(1000, ErrorMessage = "Başarılar en fazla 1000 karakter olabilir")]
    public string? Achievements { get; set; }

    public DateTime? StartDate { get; set; }
}
