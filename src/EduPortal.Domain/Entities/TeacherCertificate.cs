using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class TeacherCertificate
{
    [Key]
    public int Id { get; set; }

    public int TeacherId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Institution { get; set; } = string.Empty;

    public DateTime? IssueDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    [MaxLength(500)]
    public string? FileUrl { get; set; }

    [ForeignKey(nameof(TeacherId))]
    public virtual Teacher? Teacher { get; set; }
}
