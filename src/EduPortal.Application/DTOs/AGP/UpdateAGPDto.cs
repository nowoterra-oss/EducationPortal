using System.ComponentModel.DataAnnotations;
using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.AGP;

public class UpdateAGPDto
{
    [Required(ErrorMessage = "Akademik yıl belirtilmelidir")]
    [MaxLength(20, ErrorMessage = "Akademik yıl en fazla 20 karakter olabilir")]
    public string AcademicYear { get; set; } = string.Empty;

    [Required(ErrorMessage = "Başlangıç tarihi belirtilmelidir")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Bitiş tarihi belirtilmelidir")]
    public DateTime EndDate { get; set; }

    [MaxLength(500, ErrorMessage = "Plan doküman URL en fazla 500 karakter olabilir")]
    public string? PlanDocumentUrl { get; set; }

    [Required(ErrorMessage = "Durum belirtilmelidir")]
    public AGPStatus Status { get; set; }
}
