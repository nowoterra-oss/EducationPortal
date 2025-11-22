namespace EduPortal.Application.DTOs.Document;

public class ApplicationDocumentDto
{
    public int Id { get; set; }
    public int ProgramId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string TargetUniversity { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentUrl { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? SubmissionDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }
    public bool IsExpired { get; set; }
    public int DaysUntilExpiry { get; set; }
    public DateTime CreatedDate { get; set; }
}
