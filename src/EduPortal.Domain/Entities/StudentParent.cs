using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class StudentParent : BaseEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int ParentId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Relationship { get; set; } = string.Empty; // "Anne", "Baba", "Vasi"

    public bool IsPrimaryContact { get; set; } = false;

    public bool IsEmergencyContact { get; set; } = false;

    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    [ForeignKey(nameof(ParentId))]
    public virtual Parent Parent { get; set; } = null!;
}
