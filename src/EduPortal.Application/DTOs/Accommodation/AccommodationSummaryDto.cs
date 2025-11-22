namespace EduPortal.Application.DTOs.Accommodation;

public class AccommodationSummaryDto
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? MonthlyCost { get; set; }
}
