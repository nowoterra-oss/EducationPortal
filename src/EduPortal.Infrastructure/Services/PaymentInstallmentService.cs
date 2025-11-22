using EduPortal.Application.DTOs.PaymentPlan;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class PaymentInstallmentService : IPaymentInstallmentService
{
    private readonly ApplicationDbContext _context;

    public PaymentInstallmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PaymentInstallmentDto>> GetByStudentPaymentPlanAsync(int planId)
    {
        return await _context.Set<PaymentInstallment>()
            .Include(i => i.StudentPaymentPlan)
                .ThenInclude(p => p.Student)
                .ThenInclude(s => s.User)
            .Where(i => i.StudentPaymentPlanId == planId)
            .OrderBy(i => i.InstallmentNumber)
            .Select(i => MapToDto(i))
            .ToListAsync();
    }

    public async Task<IEnumerable<PaymentInstallmentDto>> GetByStudentIdAsync(int studentId)
    {
        return await _context.Set<PaymentInstallment>()
            .Include(i => i.StudentPaymentPlan)
                .ThenInclude(p => p.Student)
                .ThenInclude(s => s.User)
            .Where(i => i.StudentPaymentPlan.StudentId == studentId)
            .OrderBy(i => i.DueDate)
            .Select(i => MapToDto(i))
            .ToListAsync();
    }

    public async Task<PaymentInstallmentDto?> GetByIdAsync(int id)
    {
        var installment = await _context.Set<PaymentInstallment>()
            .Include(i => i.StudentPaymentPlan)
                .ThenInclude(p => p.Student)
                .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(i => i.Id == id);

        return installment != null ? MapToDto(installment) : null;
    }

    public async Task<IEnumerable<PaymentInstallmentDto>> GetOverdueAsync()
    {
        var today = DateTime.Today;

        // Önce pending olanları bul ve overdue yap
        var pendingInstallments = await _context.Set<PaymentInstallment>()
            .Where(i => i.Status == InstallmentStatus.Pending && i.DueDate < today)
            .ToListAsync();

        foreach (var inst in pendingInstallments)
        {
            inst.Status = InstallmentStatus.Overdue;
        }

        if (pendingInstallments.Any())
            await _context.SaveChangesAsync();

        // Tüm overdue'ları getir
        return await _context.Set<PaymentInstallment>()
            .Include(i => i.StudentPaymentPlan)
                .ThenInclude(p => p.Student)
                .ThenInclude(s => s.User)
            .Where(i => i.Status == InstallmentStatus.Overdue)
            .OrderBy(i => i.DueDate)
            .Select(i => MapToDto(i))
            .ToListAsync();
    }

    public async Task<IEnumerable<PaymentInstallmentDto>> GetUpcomingAsync(int days = 7)
    {
        var today = DateTime.Today;
        var futureDate = today.AddDays(days);

        return await _context.Set<PaymentInstallment>()
            .Include(i => i.StudentPaymentPlan)
                .ThenInclude(p => p.Student)
                .ThenInclude(s => s.User)
            .Where(i => i.Status == InstallmentStatus.Pending &&
                       i.DueDate >= today &&
                       i.DueDate <= futureDate)
            .OrderBy(i => i.DueDate)
            .Select(i => MapToDto(i))
            .ToListAsync();
    }

    public async Task<PaymentInstallmentDto> PayInstallmentAsync(int installmentId, PayInstallmentDto dto)
    {
        var installment = await _context.Set<PaymentInstallment>()
            .Include(i => i.StudentPaymentPlan)
            .FirstOrDefaultAsync(i => i.Id == installmentId);

        if (installment == null)
            throw new Exception($"Installment with ID {installmentId} not found");

        if (installment.Status == InstallmentStatus.Paid)
            throw new Exception("This installment is already paid");

        // Payment kaydı oluştur
        var payment = new Payment
        {
            StudentId = installment.StudentPaymentPlan.StudentId,
            Amount = dto.Amount,
            PaymentDate = dto.PaymentDate,
            PaymentMethod = dto.PaymentMethod ?? "Cash",
            TransactionReference = dto.TransactionReference,
            Status = "Completed",
            Notes = dto.Notes
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // Taksiti güncelle
        installment.PaidAmount += dto.Amount;
        installment.PaidDate = dto.PaymentDate;
        installment.PaymentId = payment.Id;

        if (installment.PaidAmount >= installment.Amount)
        {
            installment.Status = InstallmentStatus.Paid;
        }
        else
        {
            installment.Status = InstallmentStatus.PartiallyPaid;
        }

        // Student payment planı güncelle
        var plan = installment.StudentPaymentPlan;
        plan.PaidAmount += dto.Amount;
        plan.RemainingAmount = plan.TotalAmount - plan.PaidAmount;

        // Tüm taksitler ödendiyse planı tamamla
        var allInstallments = await _context.Set<PaymentInstallment>()
            .Where(i => i.StudentPaymentPlanId == plan.Id)
            .ToListAsync();

        if (allInstallments.All(i => i.Status == InstallmentStatus.Paid))
        {
            plan.Status = PaymentPlanStatus.Completed;
            plan.EndDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(installmentId))!;
    }

    public async Task<object> GetStatisticsAsync()
    {
        var installments = await _context.Set<PaymentInstallment>()
            .Include(i => i.StudentPaymentPlan)
            .ToListAsync();

        var today = DateTime.Today;

        return new
        {
            TotalInstallments = installments.Count,
            PaidInstallments = installments.Count(i => i.Status == InstallmentStatus.Paid),
            PendingInstallments = installments.Count(i => i.Status == InstallmentStatus.Pending),
            OverdueInstallments = installments.Count(i => i.Status == InstallmentStatus.Overdue),
            PartiallyPaidInstallments = installments.Count(i => i.Status == InstallmentStatus.PartiallyPaid),
            TotalAmount = installments.Sum(i => i.Amount),
            TotalPaid = installments.Sum(i => i.PaidAmount),
            TotalRemaining = installments.Sum(i => i.Amount - i.PaidAmount),
            UpcomingThisWeek = installments.Count(i =>
                i.Status == InstallmentStatus.Pending &&
                i.DueDate >= today &&
                i.DueDate <= today.AddDays(7)),
            UpcomingThisMonth = installments.Count(i =>
                i.Status == InstallmentStatus.Pending &&
                i.DueDate >= today &&
                i.DueDate <= today.AddDays(30))
        };
    }

    private static PaymentInstallmentDto MapToDto(PaymentInstallment installment)
    {
        var today = DateTime.Today;
        var daysOverdue = installment.Status == InstallmentStatus.Overdue && installment.DueDate < today
            ? (today - installment.DueDate).Days
            : 0;

        return new PaymentInstallmentDto
        {
            Id = installment.Id,
            StudentPaymentPlanId = installment.StudentPaymentPlanId,
            StudentId = installment.StudentPaymentPlan.StudentId,
            StudentName = $"{installment.StudentPaymentPlan.Student?.User?.FirstName} {installment.StudentPaymentPlan.Student?.User?.LastName}",
            StudentNo = installment.StudentPaymentPlan.Student?.StudentNo,
            InstallmentNumber = installment.InstallmentNumber,
            Amount = installment.Amount,
            PaidAmount = installment.PaidAmount,
            DueDate = installment.DueDate,
            PaidDate = installment.PaidDate,
            Status = installment.Status.ToString(),
            PaymentId = installment.PaymentId,
            Notes = installment.Notes,
            DaysOverdue = daysOverdue
        };
    }
}
