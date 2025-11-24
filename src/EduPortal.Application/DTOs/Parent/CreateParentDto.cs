using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Parent;

public class CreateParentDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Occupation { get; set; }

    [MaxLength(20)]
    public string? WorkPhone { get; set; }

    // Opsiyonel: Oluşturma sırasında öğrenci ilişkilendirmesi
    public List<StudentRelationshipDto>? StudentRelationships { get; set; }
}

public class StudentRelationshipDto
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Relationship { get; set; } = string.Empty; // "Anne", "Baba", "Vasi"

    public bool IsPrimaryContact { get; set; } = false;

    public bool IsEmergencyContact { get; set; } = false;
}
