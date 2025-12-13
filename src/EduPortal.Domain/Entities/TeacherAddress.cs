using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class TeacherAddress
{
    [Key]
    public int Id { get; set; }

    public int TeacherId { get; set; }

    [MaxLength(500)]
    public string? Street { get; set; }

    [MaxLength(100)]
    public string? District { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [ForeignKey(nameof(TeacherId))]
    public virtual Teacher? Teacher { get; set; }
}
