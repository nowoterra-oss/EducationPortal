using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Audit;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EduPortal.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuditService> _logger;

    public AuditService(ApplicationDbContext context, ILogger<AuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<long>> LogAsync(
        CreateAuditLogDto dto,
        string? userId = null,
        string? userName = null,
        string? userEmail = null)
    {
        try
        {
            var changes = CalculateChanges(dto.OldValues, dto.NewValues);

            var auditLog = new AuditLog
            {
                UserId = userId,
                UserName = userName,
                UserEmail = userEmail,
                EntityType = dto.EntityType,
                EntityId = dto.EntityId,
                Action = dto.Action,
                OldValues = dto.OldValues != null ? JsonSerializer.Serialize(dto.OldValues) : null,
                NewValues = dto.NewValues != null ? JsonSerializer.Serialize(dto.NewValues) : null,
                Changes = changes,
                IpAddress = dto.IpAddress,
                UserAgent = dto.UserAgent,
                IsSuccessful = dto.IsSuccessful,
                ErrorMessage = dto.ErrorMessage,
                AdditionalInfo = dto.AdditionalInfo,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            return ApiResponse<long>.SuccessResponse(auditLog.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit log");
            return ApiResponse<long>.ErrorResponse($"Audit log oluşturulamadı: {ex.Message}");
        }
    }

    public async Task<ApiResponse<long>> LogAsync(
        string action,
        string entityType,
        string? entityId,
        object? oldValues = null,
        object? newValues = null)
    {
        var dto = new CreateAuditLogDto
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues
        };

        return await LogAsync(dto);
    }

    public async Task<ApiResponse<PagedResponse<AuditLogDto>>> GetLogsAsync(AuditLogFilterDto filter)
    {
        try
        {
            var query = _context.AuditLogs.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.UserId))
                query = query.Where(a => a.UserId == filter.UserId);

            if (!string.IsNullOrEmpty(filter.EntityType))
                query = query.Where(a => a.EntityType == filter.EntityType);

            if (!string.IsNullOrEmpty(filter.EntityId))
                query = query.Where(a => a.EntityId == filter.EntityId);

            if (!string.IsNullOrEmpty(filter.Action))
                query = query.Where(a => a.Action == filter.Action);

            if (filter.StartDate.HasValue)
                query = query.Where(a => a.Timestamp >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(a => a.Timestamp <= filter.EndDate.Value);

            if (filter.IsSuccessful.HasValue)
                query = query.Where(a => a.IsSuccessful == filter.IsSuccessful.Value);

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var dtos = logs.Select(MapToDto).ToList();

            var pagedResponse = new PagedResponse<AuditLogDto>
            {
                Data = dtos,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            };

            return ApiResponse<PagedResponse<AuditLogDto>>.SuccessResponse(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs");
            return ApiResponse<PagedResponse<AuditLogDto>>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<AuditLogDto>>> GetUserLogsAsync(string userId, int count = 50)
    {
        try
        {
            var logs = await _context.AuditLogs
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToListAsync();

            var dtos = logs.Select(MapToDto).ToList();
            return ApiResponse<List<AuditLogDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting user logs for {userId}");
            return ApiResponse<List<AuditLogDto>>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<AuditLogDto>>> GetEntityLogsAsync(string entityType, string entityId, int count = 50)
    {
        try
        {
            var logs = await _context.AuditLogs
                .Where(a => a.EntityType == entityType && a.EntityId == entityId)
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToListAsync();

            var dtos = logs.Select(MapToDto).ToList();
            return ApiResponse<List<AuditLogDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting entity logs for {entityType}/{entityId}");
            return ApiResponse<List<AuditLogDto>>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<AuditLogDto>> GetLogByIdAsync(long id)
    {
        try
        {
            var log = await _context.AuditLogs.FindAsync(id);
            if (log == null)
                return ApiResponse<AuditLogDto>.ErrorResponse("Audit log bulunamadı");

            var dto = MapToDto(log);
            return ApiResponse<AuditLogDto>.SuccessResponse(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting audit log {id}");
            return ApiResponse<AuditLogDto>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<AuditStatisticsDto>> GetStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.AuditLogs.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(a => a.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.Timestamp <= endDate.Value);

            var today = DateTime.UtcNow.Date;

            var stats = new AuditStatisticsDto
            {
                TotalLogs = await query.CountAsync(),
                TodayLogs = await query.CountAsync(a => a.Timestamp.Date == today),
                SuccessfulLogs = await query.CountAsync(a => a.IsSuccessful),
                FailedLogs = await query.CountAsync(a => !a.IsSuccessful)
            };

            // Action breakdown
            stats.ActionBreakdown = await query
                .GroupBy(a => a.Action)
                .Select(g => new { Action = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Action, x => x.Count);

            // Entity type breakdown
            stats.EntityTypeBreakdown = await query
                .GroupBy(a => a.EntityType)
                .Select(g => new { EntityType = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.EntityType, x => x.Count);

            // Top users
            stats.TopUsers = await query
                .Where(a => a.UserId != null)
                .GroupBy(a => new { a.UserId, a.UserName, a.UserEmail })
                .Select(g => new TopUserActivityDto
                {
                    UserId = g.Key.UserId,
                    UserName = g.Key.UserName,
                    UserEmail = g.Key.UserEmail,
                    ActivityCount = g.Count(),
                    LastActivity = g.Max(a => a.Timestamp)
                })
                .OrderByDescending(x => x.ActivityCount)
                .Take(10)
                .ToListAsync();

            // Recent failures
            stats.RecentFailures = await query
                .Where(a => !a.IsSuccessful)
                .OrderByDescending(a => a.Timestamp)
                .Take(10)
                .Select(a => new RecentFailedLogDto
                {
                    Id = a.Id,
                    UserName = a.UserName,
                    Action = a.Action,
                    EntityType = a.EntityType,
                    ErrorMessage = a.ErrorMessage,
                    Timestamp = a.Timestamp
                })
                .ToListAsync();

            return ApiResponse<AuditStatisticsDto>.SuccessResponse(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit statistics");
            return ApiResponse<AuditStatisticsDto>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<AuditLogDto>>> GetRecentFailedLogsAsync(int count = 20)
    {
        try
        {
            var logs = await _context.AuditLogs
                .Where(a => !a.IsSuccessful)
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToListAsync();

            var dtos = logs.Select(MapToDto).ToList();
            return ApiResponse<List<AuditLogDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent failed logs");
            return ApiResponse<List<AuditLogDto>>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<int>> CleanupOldLogsAsync(int daysToKeep = 90)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

            var oldLogs = await _context.AuditLogs
                .Where(a => a.Timestamp < cutoffDate)
                .ToListAsync();

            _context.AuditLogs.RemoveRange(oldLogs);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Cleaned up {oldLogs.Count} old audit logs");
            return ApiResponse<int>.SuccessResponse(oldLogs.Count, $"{oldLogs.Count} eski log silindi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old logs");
            return ApiResponse<int>.ErrorResponse(ex.Message);
        }
    }

    private AuditLogDto MapToDto(AuditLog log)
    {
        return new AuditLogDto
        {
            Id = log.Id,
            UserId = log.UserId,
            UserName = log.UserName,
            UserEmail = log.UserEmail,
            EntityType = log.EntityType,
            EntityId = log.EntityId,
            Action = log.Action,
            OldValues = log.OldValues,
            NewValues = log.NewValues,
            Changes = log.Changes,
            IpAddress = log.IpAddress,
            UserAgent = log.UserAgent,
            Timestamp = log.Timestamp,
            IsSuccessful = log.IsSuccessful,
            ErrorMessage = log.ErrorMessage,
            AdditionalInfo = log.AdditionalInfo
        };
    }

    private string? CalculateChanges(object? oldValues, object? newValues)
    {
        if (oldValues == null || newValues == null)
            return null;

        try
        {
            var oldJson = JsonSerializer.Serialize(oldValues);
            var newJson = JsonSerializer.Serialize(newValues);

            if (oldJson == newJson)
                return null;

            var changes = new Dictionary<string, object>();

            var oldDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(oldJson);
            var newDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(newJson);

            if (oldDict == null || newDict == null)
                return null;

            foreach (var key in newDict.Keys)
            {
                if (!oldDict.ContainsKey(key))
                {
                    changes[key] = new { Old = (object?)null, New = newDict[key].ToString() };
                }
                else if (oldDict[key].ToString() != newDict[key].ToString())
                {
                    changes[key] = new { Old = oldDict[key].ToString(), New = newDict[key].ToString() };
                }
            }

            return changes.Any() ? JsonSerializer.Serialize(changes) : null;
        }
        catch
        {
            return null;
        }
    }
}
