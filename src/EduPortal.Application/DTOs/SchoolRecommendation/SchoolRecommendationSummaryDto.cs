namespace EduPortal.Application.DTOs.SchoolRecommendation;

public class SchoolRecommendationSummaryDto
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string SchoolName { get; set; } = string.Empty;
    public string SchoolLevel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? RankingScore { get; set; }
}
