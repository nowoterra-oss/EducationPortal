using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.SimpleInternship;

public class SimpleInternshipDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateSimpleInternshipDto
{
    [Required]
    [StringLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Position { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }
}
