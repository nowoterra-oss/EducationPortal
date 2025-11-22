namespace EduPortal.Application.DTOs.Visa;

public class VisaTimelineDto
{
    public int VisaId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string TargetCountry { get; set; } = string.Empty;
    public List<VisaTimelineEventDto> Events { get; set; } = new();
}

public class VisaTimelineEventDto
{
    public string Stage { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public string? Notes { get; set; }
}
