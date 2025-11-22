namespace EduPortal.Application.DTOs.Accommodation;

public class AccommodationStatisticsDto
{
    public int TotalArrangements { get; set; }
    public int ActiveArrangements { get; set; }
    public int ConfirmedArrangements { get; set; }
    public int PendingArrangements { get; set; }

    public Dictionary<string, int> ArrangementsByType { get; set; } = new();
    public Dictionary<string, int> ArrangementsByStatus { get; set; } = new();
    public Dictionary<string, int> ArrangementsByCountry { get; set; } = new();

    public decimal TotalMonthlyCosts { get; set; }
    public decimal TotalSecurityDeposits { get; set; }
    public decimal AverageMonthlyCost { get; set; }
}
