using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class StudentSibling : BaseEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    [MaxLength(200)]
    public string SiblingName { get; set; } = string.Empty;

    [Range(1, 12)]
    public int? Grade { get; set; }

    [MaxLength(200)]
    public string? School { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
}
