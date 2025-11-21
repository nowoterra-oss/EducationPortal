using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Branch;

public class CreateBranchDto
{
    [Required]
    [MaxLength(200)]
    public string BranchName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string BranchCode { get; set; } = string.Empty;

    [Required]
    public int Type { get; set; } // 0: Main, 1: Branch, 2: Satellite

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? District { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    [EmailAddress]
    public string? Email { get; set; }

    public string? ManagerId { get; set; }

    [Required]
    [Range(1, 10000)]
    public int Capacity { get; set; }

    [Required]
    public DateTime OpeningDate { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
