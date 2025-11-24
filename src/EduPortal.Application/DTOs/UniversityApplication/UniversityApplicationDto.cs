using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.UniversityApplication;

public class UniversityApplicationDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string UniversityName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string? RequirementsUrl { get; set; }
    public DateTime? ApplicationStartDate { get; set; }
    public DateTime ApplicationDeadline { get; set; }
    public DateTime? DecisionDate { get; set; }
    public ApplicationStatus Status { get; set; }
    public string StatusName => GetStatusName(Status);
    public string? Notes { get; set; }
    public int DaysUntilDeadline => (ApplicationDeadline - DateTime.Today).Days;
    public bool IsDeadlinePassed => ApplicationDeadline < DateTime.Today;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    private static string GetStatusName(ApplicationStatus status) => status switch
    {
        ApplicationStatus.Planlaniyor => "Planlanıyor",
        ApplicationStatus.BasvuruYapildi => "Başvuru Yapıldı",
        ApplicationStatus.Kabul => "Kabul Edildi",
        ApplicationStatus.Red => "Reddedildi",
        ApplicationStatus.Beklemede => "Beklemede",
        _ => status.ToString()
    };
}
