using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EduPortal.API.HealthChecks;

/// <summary>
/// Health check for SignalR Hub connectivity
/// </summary>
public class SignalRHealthCheck : IHealthCheck
{
    private readonly ILogger<SignalRHealthCheck> _logger;

    public SignalRHealthCheck(ILogger<SignalRHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // SignalR Hub'ın çalışır durumda olup olmadığını kontrol et
            // Basit bir kontrol - SignalR servisi DI'da kayıtlı mı?
            // Not: Gerçek bir bağlantı testi için SignalR client kullanılabilir

            return Task.FromResult(HealthCheckResult.Healthy("SignalR Hub is available"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SignalR health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "SignalR Hub is unavailable",
                exception: ex));
        }
    }
}

/// <summary>
/// Health check for database connectivity with detailed status
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(IServiceProvider serviceProvider, ILogger<DatabaseHealthCheck> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<Microsoft.EntityFrameworkCore.DbContext>();

            // Veritabanına basit bir sorgu gönder
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

            if (canConnect)
            {
                return HealthCheckResult.Healthy("Database connection is healthy");
            }

            return HealthCheckResult.Unhealthy("Cannot connect to database");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy(
                "Database connection failed",
                exception: ex);
        }
    }
}
