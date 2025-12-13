using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class TeacherReference
{
    [Key]
    public int Id { get; set; }

    public int TeacherId { get; set; }

    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Organization { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? PhoneNumber { get; set; }

    [MaxLength(200)]
    public string? Email { get; set; }

    [ForeignKey(nameof(TeacherId))]
    public virtual Teacher? Teacher { get; set; }
}
