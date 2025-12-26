namespace EduPortal.Application.DTOs.SchoolRecommendation;

public class SchoolRecommendationDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentNo { get; set; } = string.Empty;
    public int CounselorId { get; set; }
    public string CounselorName { get; set; } = string.Empty;
    public string SchoolName { get; set; } = string.Empty;
    public string SchoolLevel { get; set; } = string.Empty;
    public string SchoolType { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? District { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reasoning { get; set; }
    public int? RankingScore { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}
