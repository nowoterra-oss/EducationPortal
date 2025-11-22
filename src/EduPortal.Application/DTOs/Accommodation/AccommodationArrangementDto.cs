namespace EduPortal.Application.DTOs.Accommodation;

public class AccommodationArrangementDto
{
    public int Id { get; set; }
    public int ProgramId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentNo { get; set; } = string.Empty;
    public string TargetCountry { get; set; } = string.Empty;
    public string TargetUniversity { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Address { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? MonthlyCost { get; set; }
    public decimal? SecurityDeposit { get; set; }
    public decimal? TotalCost { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? Notes { get; set; }
    public int DurationMonths { get; set; }
    public bool IsActive { get; set; }
    public int DaysUntilStart { get; set; }
    public int DaysUntilEnd { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}
