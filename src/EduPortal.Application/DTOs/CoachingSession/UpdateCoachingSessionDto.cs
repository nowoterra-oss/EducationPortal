using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.CoachingSession;

public class UpdateCoachingSessionDto
{
    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateTime SessionDate { get; set; }

    [Required]
    [Range(15, 180)]
    public int DurationMinutes { get; set; }

    [Required]
    public int SessionType { get; set; }

    [Required]
    public int Status { get; set; } // SessionStatus enum value

    [MaxLength(3000)]
    public string? SessionNotes { get; set; }

    [MaxLength(2000)]
    public string? ActionItems { get; set; }
}
