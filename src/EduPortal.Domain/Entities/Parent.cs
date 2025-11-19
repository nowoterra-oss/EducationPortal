using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class Parent : BaseEntity
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int StudentId { get; set; }

    [Required]
    [MaxLength(50)]
    public string ParentType { get; set; } = string.Empty; // "Anne", "Baba", "Diger"

    [MaxLength(200)]
    public string? Occupation { get; set; }

    public bool IsParentsSeparated { get; set; } = false;

    // Navigation Properties
    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; } = null!;

    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
}
