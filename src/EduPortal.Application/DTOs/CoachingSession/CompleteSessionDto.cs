using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.CoachingSession;

public class CompleteSessionDto
{
    [Required]
    [MaxLength(3000)]
    public string SessionNotes { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? ActionItems { get; set; }

    [MaxLength(2000)]
    public string? StudentFeedback { get; set; }

    [Range(1, 5)]
    public int? Rating { get; set; }

    public DateTime? NextSessionDate { get; set; }
}
