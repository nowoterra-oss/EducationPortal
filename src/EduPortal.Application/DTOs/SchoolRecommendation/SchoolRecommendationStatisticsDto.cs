namespace EduPortal.Application.DTOs.SchoolRecommendation;

public class SchoolRecommendationStatisticsDto
{
    public int TotalRecommendations { get; set; }
    public int AcceptedRecommendations { get; set; }
    public int PendingRecommendations { get; set; }
    public int RejectedRecommendations { get; set; }

    public Dictionary<string, int> RecommendationsByLevel { get; set; } = new();
    public Dictionary<string, int> RecommendationsByType { get; set; } = new();
    public Dictionary<string, int> RecommendationsByCity { get; set; } = new();
    public Dictionary<string, int> RecommendationsByStatus { get; set; } = new();

    public decimal AverageRankingScore { get; set; }
}
