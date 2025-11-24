using EduPortal.Application.DTOs.Payment;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;

    public PaymentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<PaymentSummaryDto> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize)
    {
        var query = _context.Payments
            .Include(p => p.Student)
                .ThenInclude(s => s.User)
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.PaymentDate);

        var totalCount = await query.CountAsync();

        var payments = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = payments.Select(MapToSummaryDto);

        return (items, totalCount);
    }

    public async Task<PaymentDto?> GetByIdAsync(int id)
    {
        var payment = await _context.Payments
            .Include(p => p.Student)
                .ThenInclude(s => s.User)
            .Include(p => p.Branch)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        return payment != null ? MapToDto(payment) : null;
    }

    public async Task<PaymentDto> CreateAsync(PaymentCreateDto dto)
    {
        var payment = new Payment
        {
            StudentId = dto.StudentId,
            InstallmentId = dto.InstallmentId,
            PaymentPlanId = dto.PaymentPlanId,
            BranchId = dto.BranchId,
            Amount = dto.Amount,
            PaymentDate = dto.PaymentDate,
            PaymentMethod = dto.PaymentMethod,
            Status = PaymentStatus.Bekliyor,
            TransactionId = dto.TransactionId,
            ReceiptNumber = dto.ReceiptNumber ?? GenerateReceiptNumber(),
            Description = dto.Description,
            Notes = dto.Notes
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(payment.Id))!;
    }

    public async Task<PaymentDto> UpdateAsync(int id, PaymentCreateDto dto)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null || payment.IsDeleted)
            throw new Exception("Payment not found");

        payment.Amount = dto.Amount;
        payment.PaymentDate = dto.PaymentDate;
        payment.PaymentMethod = dto.PaymentMethod;
        payment.TransactionId = dto.TransactionId;
        payment.Description = dto.Description;
        payment.Notes = dto.Notes;
        payment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(id))!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null || payment.IsDeleted)
            return false;

        payment.IsDeleted = true;
        payment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<(IEnumerable<PaymentSummaryDto> Items, int TotalCount)> GetByStudentPagedAsync(int studentId, int pageNumber, int pageSize)
    {
        var query = _context.Payments
            .Include(p => p.Student)
                .ThenInclude(s => s.User)
            .Where(p => p.StudentId == studentId && !p.IsDeleted)
            .OrderByDescending(p => p.PaymentDate);

        var totalCount = await query.CountAsync();

        var payments = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = payments.Select(MapToSummaryDto);

        return (items, totalCount);
    }

    public async Task<(IEnumerable<PaymentSummaryDto> Items, int TotalCount)> GetByStatusPagedAsync(PaymentStatus status, int pageNumber, int pageSize)
    {
        var query = _context.Payments
            .Include(p => p.Student)
                .ThenInclude(s => s.User)
            .Where(p => p.Status == status && !p.IsDeleted)
            .OrderByDescending(p => p.PaymentDate);

        var totalCount = await query.CountAsync();

        var payments = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = payments.Select(MapToSummaryDto);

        return (items, totalCount);
    }

    public async Task<(IEnumerable<PaymentSummaryDto> Items, int TotalCount)> GetPendingPagedAsync(int pageNumber, int pageSize)
    {
        return await GetByStatusPagedAsync(PaymentStatus.Bekliyor, pageNumber, pageSize);
    }

    public async Task<(IEnumerable<PaymentSummaryDto> Items, int TotalCount)> GetOverduePagedAsync(int pageNumber, int pageSize)
    {
        // Gecikmiş ödemeler - hem GecikmisPaid status'ü hem de Bekliyor olup tarihi geçmiş olanlar
        var today = DateTime.Today;

        var query = _context.Payments
            .Include(p => p.Student)
                .ThenInclude(s => s.User)
            .Include(p => p.Installment)
            .Where(p => !p.IsDeleted && (
                p.Status == PaymentStatus.GecikmisPaid ||
                (p.Status == PaymentStatus.Bekliyor && p.Installment != null && p.Installment.DueDate < today)
            ))
            .OrderByDescending(p => p.PaymentDate);

        var totalCount = await query.CountAsync();

        var payments = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = payments.Select(MapToSummaryDto);

        return (items, totalCount);
    }

    public async Task<PaymentDto> ProcessPaymentAsync(int id)
    {
        var payment = await _context.Payments
            .Include(p => p.Installment)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (payment == null)
            throw new Exception("Payment not found");

        if (payment.Status == PaymentStatus.Odendi)
            throw new Exception("Payment already processed");

        payment.Status = PaymentStatus.Odendi;
        payment.ProcessedAt = DateTime.UtcNow;

        // İlişkili taksit varsa güncelle
        if (payment.Installment != null)
        {
            payment.Installment.PaidAmount += payment.Amount;
            payment.Installment.PaidDate = DateTime.UtcNow;

            if (payment.Installment.PaidAmount >= payment.Installment.Amount)
            {
                payment.Installment.Status = InstallmentStatus.Paid;
            }
            else
            {
                payment.Installment.Status = InstallmentStatus.PartiallyPaid;
            }
        }

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(id))!;
    }

    public async Task<PaymentStatisticsDto> GetStatisticsAsync()
    {
        var payments = await _context.Payments
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        var today = DateTime.Today;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var completedPayments = payments.Where(p => p.Status == PaymentStatus.Odendi).ToList();

        return new PaymentStatisticsDto
        {
            TotalPayments = payments.Count,
            PendingPayments = payments.Count(p => p.Status == PaymentStatus.Bekliyor),
            CompletedPayments = completedPayments.Count,
            OverduePayments = payments.Count(p => p.Status == PaymentStatus.GecikmisPaid),
            CancelledPayments = payments.Count(p => p.Status == PaymentStatus.Iptal),

            TotalAmount = payments.Sum(p => p.Amount),
            TotalPaidAmount = completedPayments.Sum(p => p.Amount),
            TotalPendingAmount = payments.Where(p => p.Status == PaymentStatus.Bekliyor).Sum(p => p.Amount),
            TotalOverdueAmount = payments.Where(p => p.Status == PaymentStatus.GecikmisPaid).Sum(p => p.Amount),

            // Ödeme yöntemi dağılımı
            CashAmount = completedPayments.Where(p => p.PaymentMethod == PaymentMethod.Nakit).Sum(p => p.Amount),
            CreditCardAmount = completedPayments.Where(p => p.PaymentMethod == PaymentMethod.Kredi).Sum(p => p.Amount),
            TransferAmount = completedPayments.Where(p => p.PaymentMethod == PaymentMethod.Havale).Sum(p => p.Amount),
            OtherAmount = completedPayments.Where(p => p.PaymentMethod == PaymentMethod.Diger).Sum(p => p.Amount),

            // Zaman bazlı istatistikler
            TodayAmount = completedPayments.Where(p => p.PaymentDate.Date == today).Sum(p => p.Amount),
            ThisWeekAmount = completedPayments.Where(p => p.PaymentDate.Date >= weekStart).Sum(p => p.Amount),
            ThisMonthAmount = completedPayments.Where(p => p.PaymentDate.Date >= monthStart).Sum(p => p.Amount),

            TodayPaymentCount = completedPayments.Count(p => p.PaymentDate.Date == today),
            ThisWeekPaymentCount = completedPayments.Count(p => p.PaymentDate.Date >= weekStart),
            ThisMonthPaymentCount = completedPayments.Count(p => p.PaymentDate.Date >= monthStart)
        };
    }

    public async Task<byte[]?> GenerateReceiptAsync(int id)
    {
        var payment = await GetByIdAsync(id);
        if (payment == null)
            return null;

        // Basit bir text-based receipt oluştur (gerçek uygulamada PDF library kullanılabilir)
        var receiptContent = $@"
=======================================
           ÖDEME MAKBUZU
=======================================
Makbuz No: {payment.ReceiptNumber}
Tarih: {payment.PaymentDate:dd.MM.yyyy HH:mm}

Öğrenci: {payment.StudentName}
Öğrenci No: {payment.StudentNo}

Tutar: {payment.Amount:N2} TL
Ödeme Yöntemi: {payment.PaymentMethodName}
Durum: {payment.StatusName}

{(string.IsNullOrEmpty(payment.TransactionId) ? "" : $"İşlem No: {payment.TransactionId}")}
{(string.IsNullOrEmpty(payment.Description) ? "" : $"Açıklama: {payment.Description}")}

=======================================
";
        return System.Text.Encoding.UTF8.GetBytes(receiptContent);
    }

    private static string GenerateReceiptNumber()
    {
        return $"RCP-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }

    private static PaymentDto MapToDto(Payment payment)
    {
        return new PaymentDto
        {
            Id = payment.Id,
            StudentId = payment.StudentId,
            StudentName = payment.Student?.User != null
                ? $"{payment.Student.User.FirstName} {payment.Student.User.LastName}"
                : string.Empty,
            StudentNo = payment.Student?.StudentNo,
            InstallmentId = payment.InstallmentId,
            PaymentPlanId = payment.PaymentPlanId,
            BranchId = payment.BranchId,
            BranchName = payment.Branch?.BranchName,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            Status = payment.Status,
            PaymentMethod = payment.PaymentMethod,
            TransactionId = payment.TransactionId,
            ReceiptNumber = payment.ReceiptNumber,
            Description = payment.Description,
            Notes = payment.Notes,
            ProcessedAt = payment.ProcessedAt,
            ProcessedBy = payment.ProcessedBy,
            CreatedAt = payment.CreatedAt
        };
    }

    private static PaymentSummaryDto MapToSummaryDto(Payment payment)
    {
        return new PaymentSummaryDto
        {
            Id = payment.Id,
            StudentName = payment.Student?.User != null
                ? $"{payment.Student.User.FirstName} {payment.Student.User.LastName}"
                : string.Empty,
            StudentNo = payment.Student?.StudentNo,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            Status = payment.Status.ToString(),
            PaymentMethod = payment.PaymentMethod.ToString(),
            ReceiptNumber = payment.ReceiptNumber
        };
    }
}
