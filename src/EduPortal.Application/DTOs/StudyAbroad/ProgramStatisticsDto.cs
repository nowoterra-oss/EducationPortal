namespace EduPortal.Application.DTOs.StudyAbroad;

public class ProgramStatisticsDto
{
    public int TotalPrograms { get; set; }
    public int ActivePrograms { get; set; }
    public int CompletedPrograms { get; set; }
    public int CancelledPrograms { get; set; }

    public Dictionary<string, int> ProgramsByCountry { get; set; } = new();
    public Dictionary<string, int> ProgramsByStatus { get; set; } = new();
    public Dictionary<string, int> ProgramsByLevel { get; set; } = new();

    public decimal TotalEstimatedCost { get; set; }
    public decimal TotalActualCost { get; set; }
    public decimal AverageCost { get; set; }
}
