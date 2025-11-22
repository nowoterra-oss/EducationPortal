namespace EduPortal.Application.DTOs.Document;

public class DocumentChecklistDto
{
    public int ProgramId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string TargetUniversity { get; set; } = string.Empty;
    public List<DocumentChecklistItemDto> Documents { get; set; } = new();
    public int TotalDocuments { get; set; }
    public int CompletedDocuments { get; set; }
    public int PendingDocuments { get; set; }
    public int ProgressPercentage { get; set; }
}

public class DocumentChecklistItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? SubmissionDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
