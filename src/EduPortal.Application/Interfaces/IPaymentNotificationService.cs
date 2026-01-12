namespace EduPortal.Application.Interfaces;

public interface IPaymentNotificationService
{
    Task SendPaymentPlanCreatedAsync(int studentId, decimal totalAmount, int installmentCount);

    Task SendPaymentReminderAsync(int studentId, int installmentNumber, decimal amount, DateTime dueDate, int daysBeforeDue);

    Task SendPaymentOverdueAsync(int studentId, int installmentNumber, decimal amount, DateTime dueDate, int daysOverdue);

    Task SendPaymentReceivedAsync(int studentId, decimal amount, string receiptNumber);

    Task SendBulkPaymentRemindersAsync();

    Task SendBulkOverdueNotificationsAsync();

    Task SendSalaryCreatedAsync(int teacherId, decimal netSalary, int month, int year);

    Task SendSalaryPaidAsync(int teacherId, decimal netSalary, int month, int year);
}
