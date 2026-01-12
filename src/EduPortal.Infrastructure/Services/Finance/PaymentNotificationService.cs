using EduPortal.Application.Interfaces;
using EduPortal.Application.Interfaces.Messaging;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace EduPortal.Infrastructure.Services.Finance;

public class PaymentNotificationService : IPaymentNotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IBroadcastService _broadcastService;
    private readonly ILogger<PaymentNotificationService> _logger;
    private const string SystemUserId = "SYSTEM";
    private readonly CultureInfo _culture = new("tr-TR");

    public PaymentNotificationService(
        ApplicationDbContext context,
        IBroadcastService broadcastService,
        ILogger<PaymentNotificationService> logger)
    {
        _context = context;
        _broadcastService = broadcastService;
        _logger = logger;
    }

    public async Task SendPaymentPlanCreatedAsync(int studentId, decimal totalAmount, int installmentCount)
    {
        try
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student?.User == null) return;

            var studentName = $"{student.User.FirstName} {student.User.LastName}";
            var title = $"Ödeme Planı Oluşturuldu - {studentName}";
            var content = $"{studentName} adlı öğrenci için {installmentCount} taksitli ödeme planı oluşturulmuştur.\n\n" +
                          $"Toplam Tutar: {totalAmount:N2} TL";

            await SendToStudentAndParentsAsync(student.UserId, studentId, title, content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ödeme planı bildirim gönderilirken hata oluştu. StudentId: {StudentId}", studentId);
        }
    }

    public async Task SendPaymentReminderAsync(int studentId, int installmentNumber, decimal amount, DateTime dueDate, int daysBeforeDue)
    {
        try
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student?.User == null) return;

            var studentName = $"{student.User.FirstName} {student.User.LastName}";
            var title = $"Ödeme Hatırlatması - {studentName}";
            var content = $"{studentName} adlı öğrencinin {installmentNumber}. taksiti {daysBeforeDue} gün içinde ödenmektedir.\n\n" +
                          $"Vade Tarihi: {dueDate:dd MMMM yyyy}\n" +
                          $"Tutar: {amount:N2} TL\n\n" +
                          "Lütfen ödemenizi zamanında yapınız.";

            await SendToStudentAndParentsAsync(student.UserId, studentId, title, content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ödeme hatırlatma bildirimi gönderilirken hata oluştu. StudentId: {StudentId}", studentId);
        }
    }

    public async Task SendPaymentOverdueAsync(int studentId, int installmentNumber, decimal amount, DateTime dueDate, int daysOverdue)
    {
        try
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student?.User == null) return;

            var studentName = $"{student.User.FirstName} {student.User.LastName}";
            var title = $"⚠️ GECİKMİŞ ÖDEME - {studentName}";
            var content = $"UYARI: {studentName} adlı öğrencinin {installmentNumber}. taksiti {daysOverdue} gündür gecikmiştir.\n\n" +
                          $"Vade Tarihi: {dueDate:dd MMMM yyyy}\n" +
                          $"Tutar: {amount:N2} TL\n\n" +
                          "Lütfen ödemenizi en kısa sürede yapınız.";

            await SendToStudentAndParentsAsync(student.UserId, studentId, title, content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gecikmiş ödeme bildirimi gönderilirken hata oluştu. StudentId: {StudentId}", studentId);
        }
    }

    public async Task SendPaymentReceivedAsync(int studentId, decimal amount, string receiptNumber)
    {
        try
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student?.User == null) return;

            var studentName = $"{student.User.FirstName} {student.User.LastName}";
            var title = $"✓ Ödeme Alındı - {studentName}";
            var content = $"{studentName} adlı öğrencinin ödemesi başarıyla alınmıştır.\n\n" +
                          $"Tutar: {amount:N2} TL\n" +
                          $"Makbuz No: {receiptNumber}\n\n" +
                          "Teşekkür ederiz.";

            await SendToStudentAndParentsAsync(student.UserId, studentId, title, content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ödeme alındı bildirimi gönderilirken hata oluştu. StudentId: {StudentId}", studentId);
        }
    }

    public async Task SendBulkPaymentRemindersAsync()
    {
        try
        {
            var reminderDays = 7;
            var targetDate = DateTime.Today.AddDays(reminderDays);

            var upcomingInstallments = await _context.PaymentInstallments
                .Include(i => i.StudentPaymentPlan)
                    .ThenInclude(spp => spp.Student)
                        .ThenInclude(s => s.User)
                .Where(i => i.Status == InstallmentStatus.Pending
                    && i.DueDate.Date == targetDate.Date
                    && !i.StudentPaymentPlan.IsDeleted)
                .ToListAsync();

            foreach (var installment in upcomingInstallments)
            {
                var studentId = installment.StudentPaymentPlan.StudentId;
                await SendPaymentReminderAsync(
                    studentId,
                    installment.InstallmentNumber,
                    installment.Amount - installment.PaidAmount,
                    installment.DueDate,
                    reminderDays);
            }

            _logger.LogInformation("{Count} adet ödeme hatırlatma bildirimi gönderildi.", upcomingInstallments.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Toplu ödeme hatırlatma bildirimleri gönderilirken hata oluştu.");
        }
    }

    public async Task SendBulkOverdueNotificationsAsync()
    {
        try
        {
            var today = DateTime.Today;

            var overdueInstallments = await _context.PaymentInstallments
                .Include(i => i.StudentPaymentPlan)
                    .ThenInclude(spp => spp.Student)
                        .ThenInclude(s => s.User)
                .Where(i => (i.Status == InstallmentStatus.Pending || i.Status == InstallmentStatus.Overdue)
                    && i.DueDate.Date < today
                    && !i.StudentPaymentPlan.IsDeleted)
                .ToListAsync();

            foreach (var installment in overdueInstallments)
            {
                if (installment.Status != InstallmentStatus.Overdue)
                {
                    installment.Status = InstallmentStatus.Overdue;
                }

                var daysOverdue = (today - installment.DueDate.Date).Days;

                if (daysOverdue == 1 || daysOverdue % 3 == 0)
                {
                    var studentId = installment.StudentPaymentPlan.StudentId;
                    await SendPaymentOverdueAsync(
                        studentId,
                        installment.InstallmentNumber,
                        installment.Amount - installment.PaidAmount,
                        installment.DueDate,
                        daysOverdue);
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("{Count} adet gecikmiş ödeme güncellendi/bildirildi.", overdueInstallments.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Toplu gecikmiş ödeme bildirimleri gönderilirken hata oluştu.");
        }
    }

    public async Task SendSalaryCreatedAsync(int teacherId, decimal netSalary, int month, int year)
    {
        try
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == teacherId);

            if (teacher?.User == null) return;

            var teacherName = $"{teacher.User.FirstName} {teacher.User.LastName}";
            var monthName = _culture.DateTimeFormat.GetMonthName(month);
            var title = $"Maaş Bordrosu - {monthName} {year}";
            var content = $"Sayın {teacherName},\n\n" +
                          $"{year} yılı {monthName} ayı maaş bordronuz oluşturulmuştur.\n\n" +
                          $"Net Maaş: {netSalary:N2} TL\n\n" +
                          "Detaylar için öğretmen panelinizi kontrol ediniz.";

            await _broadcastService.SendDirectBroadcastAsync(teacher.UserId, title, content, SystemUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Maaş oluşturuldu bildirimi gönderilirken hata oluştu. TeacherId: {TeacherId}", teacherId);
        }
    }

    public async Task SendSalaryPaidAsync(int teacherId, decimal netSalary, int month, int year)
    {
        try
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == teacherId);

            if (teacher?.User == null) return;

            var teacherName = $"{teacher.User.FirstName} {teacher.User.LastName}";
            var monthName = _culture.DateTimeFormat.GetMonthName(month);
            var title = $"✓ Maaş Ödendi - {monthName} {year}";
            var content = $"Sayın {teacherName},\n\n" +
                          $"{year} yılı {monthName} ayı maaşınız hesabınıza yatırılmıştır.\n\n" +
                          $"Tutar: {netSalary:N2} TL\n\n" +
                          "İyi günler dileriz.";

            await _broadcastService.SendDirectBroadcastAsync(teacher.UserId, title, content, SystemUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Maaş ödendi bildirimi gönderilirken hata oluştu. TeacherId: {TeacherId}", teacherId);
        }
    }

    private async Task SendToStudentAndParentsAsync(string studentUserId, int studentId, string title, string content)
    {
        await _broadcastService.SendDirectBroadcastAsync(studentUserId, title, content, SystemUserId);

        var parentUserIds = await _context.StudentParents
            .Where(sp => sp.StudentId == studentId)
            .Include(sp => sp.Parent)
            .Select(sp => sp.Parent.UserId)
            .ToListAsync();

        foreach (var parentUserId in parentUserIds)
        {
            await _broadcastService.SendDirectBroadcastAsync(parentUserId, title, content, SystemUserId);
        }
    }
}
