using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class CompetitionAndAward : BaseEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Category { get; set; }

    [MaxLength(100)]
    public string? Level { get; set; } // "Okul", "Ulusal", "Uluslararasi"

    [MaxLength(100)]
    public string? Achievement { get; set; } // "1. Oldu", "İkincilik", vb.

    public DateTime? Date { get; set; }

    [MaxLength(500)]
    public string? DocumentUrl { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
}
