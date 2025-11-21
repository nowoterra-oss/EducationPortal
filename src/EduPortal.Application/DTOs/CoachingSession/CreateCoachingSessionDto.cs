using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.CoachingSession;

public class CreateCoachingSessionDto
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int CoachId { get; set; }

    public int? BranchId { get; set; }

    [Required]
    public int CoachingArea { get; set; } // CoachingArea enum value

    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateTime SessionDate { get; set; }

    [Required]
    [Range(15, 180)]
    public int DurationMinutes { get; set; }

    [Required]
    public int SessionType { get; set; } // SessionType enum value

    [MaxLength(3000)]
    public string? SessionNotes { get; set; }
}
