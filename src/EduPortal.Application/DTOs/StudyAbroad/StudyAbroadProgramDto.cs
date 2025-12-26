namespace EduPortal.Application.DTOs.StudyAbroad;

public class StudyAbroadProgramDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentNo { get; set; } = string.Empty;
    public int CounselorId { get; set; }
    public string CounselorName { get; set; } = string.Empty;
    public string TargetCountry { get; set; } = string.Empty;
    public string TargetUniversity { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public DateTime IntendedStartDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Requirements { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public string? Notes { get; set; }

    // Statistics
    public int TotalDocuments { get; set; }
    public int CompletedDocuments { get; set; }
    public int PendingDocuments { get; set; }
    public bool HasVisa { get; set; }
    public string? VisaStatus { get; set; }
    public bool HasAccommodation { get; set; }
    public string? AccommodationStatus { get; set; }
    public int ProgressPercentage { get; set; }

    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}
