using EduPortal.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EduPortal.Infrastructure.BackgroundJobs;

public class HomeworkReminderJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HomeworkReminderJob> _logger;

    public HomeworkReminderJob(
        IServiceProvider serviceProvider,
        ILogger<HomeworkReminderJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("HomeworkReminderJob started at: {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Her gün saat 09:00'da çalıştır
                var now = DateTime.Now;
                var nextRun = now.Date.AddDays(now.Hour >= 9 ? 1 : 0).AddHours(9);
                var delay = nextRun - now;

                _logger.LogInformation("Next homework reminder check scheduled at: {time}", nextRun);

                await Task.Delay(delay, stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IHomeworkAssignmentService>();

                var result = await service.SendDueDateRemindersAsync();

                if (result.Success)
                {
                    _logger.LogInformation("Homework reminders sent successfully: {Count} reminders", result.Data);
                }
                else
                {
                    _logger.LogWarning("Failed to send homework reminders: {Message}", result.Message);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("HomeworkReminderJob cancelled");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in homework reminder job");
                // Hata durumunda 1 saat bekle ve tekrar dene
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        _logger.LogInformation("HomeworkReminderJob stopped at: {time}", DateTimeOffset.Now);
    }
}
