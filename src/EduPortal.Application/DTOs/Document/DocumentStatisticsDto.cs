namespace EduPortal.Application.DTOs.Document;

public class DocumentStatisticsDto
{
    public int TotalDocuments { get; set; }
    public int CompletedDocuments { get; set; }
    public int PendingDocuments { get; set; }
    public int ExpiringDocuments { get; set; }
    public int ExpiredDocuments { get; set; }

    public Dictionary<string, int> DocumentsByType { get; set; } = new();
    public Dictionary<string, int> DocumentsByStatus { get; set; } = new();
}
