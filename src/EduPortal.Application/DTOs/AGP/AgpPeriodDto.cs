namespace EduPortal.Application.DTOs.AGP;

/// <summary>
/// AGP Timeline dönem DTO'su
/// </summary>
public class AgpPeriodDto
{
    public int? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public string? Color { get; set; }
    public int Order { get; set; }
    public List<AgpMilestoneDto> Milestones { get; set; } = new();
    public List<AgpActivityDto> Activities { get; set; } = new();
}

/// <summary>
/// AGP Timeline sınav/hedef DTO'su (örn: "SAT Ağustos", "IELTS 29 Ağu.")
/// </summary>
public class AgpMilestoneDto
{
    public int? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string Type { get; set; } = "exam"; // "exam", "goal", "event"
}

/// <summary>
/// AGP Timeline aktivite DTO'su (örn: "SAT Eng- IELTS", 6 saat/hf)
/// </summary>
public class AgpActivityDto
{
    public int? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int HoursPerWeek { get; set; }
    public string? Notes { get; set; }
}
