using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Visa;

public class UpdateVisaProcessDto
{
    [Required]
    [MaxLength(100)]
    public string VisaType { get; set; } = string.Empty;

    [Required]
    public int Status { get; set; } // VisaStatus enum

    [Required]
    public DateTime ApplicationDate { get; set; }

    public DateTime? InterviewDate { get; set; }

    public DateTime? ApprovalDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    [MaxLength(500)]
    public string? Embassy { get; set; }

    public decimal? ApplicationFee { get; set; }

    [MaxLength(100)]
    public string? VisaNumber { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
