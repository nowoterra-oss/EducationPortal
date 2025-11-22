using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Visa;

public class CreateVisaProcessDto
{
    [Required]
    public int ProgramId { get; set; }

    [Required]
    [MaxLength(100)]
    public string VisaType { get; set; } = string.Empty;

    [Required]
    public DateTime ApplicationDate { get; set; }

    public DateTime? InterviewDate { get; set; }

    [MaxLength(500)]
    public string? Embassy { get; set; }

    public decimal? ApplicationFee { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
