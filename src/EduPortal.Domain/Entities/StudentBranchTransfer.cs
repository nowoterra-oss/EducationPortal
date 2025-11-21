using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class StudentBranchTransfer : BaseAuditableEntity
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
    public TransferReason Reason { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [Required]
    public string ApprovedBy { get; set; } = string.Empty;

    [Required]
    public TransferStatus Status { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    [ForeignKey(nameof(FromBranchId))]
    public virtual Branch FromBranch { get; set; } = null!;

    [ForeignKey(nameof(ToBranchId))]
    public virtual Branch ToBranch { get; set; } = null!;

    [ForeignKey(nameof(ApprovedBy))]
    public virtual ApplicationUser Approver { get; set; } = null!;
}
