using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class Parent : BaseEntity
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Occupation { get; set; }

    [MaxLength(20)]
    public string? WorkPhone { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; } = null!;

    // N:N ilişki için
    public virtual ICollection<StudentParent> Students { get; set; } = new List<StudentParent>();
}
