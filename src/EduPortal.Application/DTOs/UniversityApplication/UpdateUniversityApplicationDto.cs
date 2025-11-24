using System.ComponentModel.DataAnnotations;
using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.UniversityApplication;

public class UpdateUniversityApplicationDto
{
    [Required(ErrorMessage = "Ülke belirtilmelidir")]
    [MaxLength(100, ErrorMessage = "Ülke en fazla 100 karakter olabilir")]
    public string Country { get; set; } = string.Empty;

    [Required(ErrorMessage = "Üniversite adı belirtilmelidir")]
    [MaxLength(200, ErrorMessage = "Üniversite adı en fazla 200 karakter olabilir")]
    public string UniversityName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bölüm belirtilmelidir")]
    [MaxLength(200, ErrorMessage = "Bölüm en fazla 200 karakter olabilir")]
    public string Department { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Gereksinimler URL en fazla 500 karakter olabilir")]
    public string? RequirementsUrl { get; set; }

    public DateTime? ApplicationStartDate { get; set; }

    [Required(ErrorMessage = "Başvuru son tarihi belirtilmelidir")]
    public DateTime ApplicationDeadline { get; set; }

    public DateTime? DecisionDate { get; set; }

    [Required(ErrorMessage = "Durum belirtilmelidir")]
    public ApplicationStatus Status { get; set; }

    [MaxLength(2000, ErrorMessage = "Notlar en fazla 2000 karakter olabilir")]
    public string? Notes { get; set; }
}
