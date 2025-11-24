using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.AGP;

public class AGPDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? PlanDocumentUrl { get; set; }
    public AGPStatus Status { get; set; }
    public string StatusName => GetStatusName(Status);
    public int MilestoneCount { get; set; }
    public int CompletedMilestoneCount { get; set; }
    public int OverallProgress { get; set; }
    public List<AGPGoalDto> Milestones { get; set; } = new();

    private static string GetStatusName(AGPStatus status) => status switch
    {
        AGPStatus.Taslak => "Taslak",
        AGPStatus.Onaylandi => "Onaylandı",
        AGPStatus.Tamamlandi => "Tamamlandı",
        _ => status.ToString()
    };
}
