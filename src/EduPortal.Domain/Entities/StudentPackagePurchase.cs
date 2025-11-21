using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class StudentPackagePurchase : BaseAuditableEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int PackageId { get; set; }

    [Required]
    public DateTime PurchaseDate { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountPaid { get; set; }

    public int RemainingSessions { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation Properties
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    [ForeignKey(nameof(PackageId))]
    public virtual ServicePackage Package { get; set; } = null!;
}
