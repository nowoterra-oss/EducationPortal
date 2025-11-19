using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class StudentClubMembership : BaseEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    [MaxLength(50)]
    public string ClubType { get; set; } = string.Empty; // "OkulIci", "OkulDisi"

    [Required]
    [MaxLength(200)]
    public string ClubName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Role { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [MaxLength(500)]
    public string? MembershipDocumentUrl { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
}
