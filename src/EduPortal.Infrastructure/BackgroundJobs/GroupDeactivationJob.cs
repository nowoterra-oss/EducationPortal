using EduPortal.Application.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EduPortal.Infrastructure.BackgroundJobs;

/// <summary>
/// Son ders tarihi gecmis gruplari otomatik pasife alan background job
/// Her gun gece yarisi (00:00) calisir
/// </summary>
public class GroupDeactivationJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GroupDeactivationJob> _logger;

    public GroupDeactivationJob(
        IServiceProvider serviceProvider,
        ILogger<GroupDeactivationJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("GroupDeactivationJob started at: {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Her gun gece yarisi (00:00) calistir
                var now = DateTime.Now;
                var nextRun = now.Date.AddDays(1); // Yarin gece yarisi
                var delay = nextRun - now;

                _logger.LogInformation("Next group deactivation check scheduled at: {time}", nextRun);

                await Task.Delay(delay, stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var groupService = scope.ServiceProvider.GetRequiredService<IStudentGroupService>();

                var result = await groupService.DeactivateExpiredGroupsAsync();

                if (result.Success)
                {
                    _logger.LogInformation("Group deactivation completed: {Count} groups deactivated", result.Data);
                }
                else
                {
                    _logger.LogWarning("Failed to deactivate groups: {Message}", result.Message);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("GroupDeactivationJob cancelled");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in group deactivation job");
                // Hata durumunda 1 saat bekle ve tekrar dene
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        _logger.LogInformation("GroupDeactivationJob stopped at: {time}", DateTimeOffset.Now);
    }
}
