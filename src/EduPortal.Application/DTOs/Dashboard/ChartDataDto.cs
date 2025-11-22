namespace EduPortal.Application.DTOs.Dashboard;

public class ChartDataDto
{
    public List<string> Labels { get; set; } = new();
    public List<decimal> Data { get; set; } = new();
    public string ChartType { get; set; } = string.Empty;
    public string? Title { get; set; }
}
