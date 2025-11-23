namespace EduPortal.Application.DTOs.Audit;

public class AuditStatisticsDto
{
    public long TotalLogs { get; set; }
    public long TodayLogs { get; set; }
    public long SuccessfulLogs { get; set; }
    public long FailedLogs { get; set; }
    public Dictionary<string, int> ActionBreakdown { get; set; } = new();
    public Dictionary<string, int> EntityTypeBreakdown { get; set; } = new();
    public List<TopUserActivityDto> TopUsers { get; set; } = new();
    public List<RecentFailedLogDto> RecentFailures { get; set; } = new();
}
