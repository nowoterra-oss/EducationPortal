using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Audit;

namespace EduPortal.Application.Interfaces;

public interface IAuditService
{
    // Create audit log
    Task<ApiResponse<long>> LogAsync(CreateAuditLogDto dto, string? userId = null, string? userName = null, string? userEmail = null);
    Task<ApiResponse<long>> LogAsync(string action, string entityType, string? entityId, object? oldValues = null, object? newValues = null);

    // Query audit logs
    Task<ApiResponse<PagedResponse<AuditLogDto>>> GetLogsAsync(AuditLogFilterDto filter);
    Task<ApiResponse<List<AuditLogDto>>> GetUserLogsAsync(string userId, int count = 50);
    Task<ApiResponse<List<AuditLogDto>>> GetEntityLogsAsync(string entityType, string entityId, int count = 50);
    Task<ApiResponse<AuditLogDto>> GetLogByIdAsync(long id);

    // Statistics
    Task<ApiResponse<AuditStatisticsDto>> GetStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<List<AuditLogDto>>> GetRecentFailedLogsAsync(int count = 20);

    // Cleanup
    Task<ApiResponse<int>> CleanupOldLogsAsync(int daysToKeep = 90);
}
