using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EduPortal.Domain.Common;

namespace EduPortal.Domain.Entities;

public class StudentCertificate : BaseAuditableEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string FileType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime? IssueDate { get; set; }

    [MaxLength(255)]
    public string? IssuingOrganization { get; set; }

    /// <summary>
    /// Indicates whether this certificate was added by an admin or counselor (true) or by the student themselves (false)
    /// </summary>
    public bool IsAddedByAdmin { get; set; }

    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
}
