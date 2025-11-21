using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Branch;

public class TransferStudentDto
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int FromBranchId { get; set; }

    [Required]
    public int ToBranchId { get; set; }

    [Required]
    public DateTime TransferDate { get; set; }

    [Required]
    public int Reason { get; set; } // 0: Relocation, 1: Closer, etc.

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
