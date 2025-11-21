using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class ServicePackage : BaseAuditableEntity
{
    [Required]
    [MaxLength(200)]
    public string PackageName { get; set; } = string.Empty;

    [Required]
    public PackageType Type { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public int? SessionCount { get; set; }

    public int? ValidityMonths { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(1000)]
    public string? Includes { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual ICollection<StudentPackagePurchase> Purchases { get; set; } = new List<StudentPackagePurchase>();
}
