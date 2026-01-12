using EduPortal.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EduPortal.Infrastructure.BackgroundJobs;

public class PaymentReminderJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentReminderJob> _logger;

    public PaymentReminderJob(
        IServiceProvider serviceProvider,
        ILogger<PaymentReminderJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PaymentReminderJob started at: {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now;
                var nextRun = now.Date.AddDays(now.Hour >= 8 ? 1 : 0).AddHours(8);
                var delay = nextRun - now;

                _logger.LogInformation("Next payment reminder check scheduled at: {time}", nextRun);

                await Task.Delay(delay, stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<IPaymentNotificationService>();
                var financeService = scope.ServiceProvider.GetRequiredService<IFinanceService>();

                _logger.LogInformation("Processing payment reminders...");
                await notificationService.SendBulkPaymentRemindersAsync();

                _logger.LogInformation("Processing overdue notifications...");
                await notificationService.SendBulkOverdueNotificationsAsync();

                _logger.LogInformation("Processing recurring expenses...");
                await financeService.ProcessRecurringExpensesAsync();

                _logger.LogInformation("Payment reminder job completed successfully at: {time}", DateTimeOffset.Now);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("PaymentReminderJob cancelled");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in payment reminder job");
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        _logger.LogInformation("PaymentReminderJob stopped at: {time}", DateTimeOffset.Now);
    }
}
