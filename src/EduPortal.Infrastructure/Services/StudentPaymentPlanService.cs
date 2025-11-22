using EduPortal.Application.DTOs.PaymentPlan;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class StudentPaymentPlanService : IStudentPaymentPlanService
{
    private readonly ApplicationDbContext _context;

    public StudentPaymentPlanService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StudentPaymentPlanDto>> GetAllAsync()
    {
        return await _context.Set<StudentPaymentPlan>()
            .Include(p => p.Student).ThenInclude(s => s.User)
            .Include(p => p.PaymentPlan)
            .Include(p => p.Installments)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<StudentPaymentPlanDto?> GetByIdAsync(int id)
    {
        var plan = await _context.Set<StudentPaymentPlan>()
            .Include(p => p.Student).ThenInclude(s => s.User)
            .Include(p => p.PaymentPlan)
            .Include(p => p.Installments)
            .FirstOrDefaultAsync(p => p.Id == id);

        return plan != null ? MapToDto(plan) : null;
    }

    public async Task<IEnumerable<StudentPaymentPlanDto>> GetByStudentIdAsync(int studentId)
    {
        return await _context.Set<StudentPaymentPlan>()
            .Include(p => p.PaymentPlan)
            .Include(p => p.Installments)
            .Where(p => p.StudentId == studentId)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<StudentPaymentPlanDto?> GetActiveByStudentIdAsync(int studentId)
    {
        var plan = await _context.Set<StudentPaymentPlan>()
            .Include(p => p.Student).ThenInclude(s => s.User)
            .Include(p => p.PaymentPlan)
            .Include(p => p.Installments)
            .FirstOrDefaultAsync(p => p.StudentId == studentId && p.Status == PaymentPlanStatus.Active);

        return plan != null ? MapToDto(plan) : null;
    }

    public async Task<StudentPaymentPlanDto> CreateAsync(CreateStudentPaymentPlanDto dto)
    {
        var paymentPlan = await _context.PaymentPlans.FindAsync(dto.PaymentPlanId);
        if (paymentPlan == null)
            throw new Exception("Payment plan not found");

        // Öğrenci ödeme planı oluştur
        var studentPlan = new StudentPaymentPlan
        {
            StudentId = dto.StudentId,
            PaymentPlanId = dto.PaymentPlanId,
            TotalAmount = dto.TotalAmount,
            RemainingAmount = dto.TotalAmount,
            StartDate = dto.StartDate,
            Status = PaymentPlanStatus.Active,
            Notes = dto.Notes,
            PackagePurchaseId = dto.PackagePurchaseId
        };

        _context.Set<StudentPaymentPlan>().Add(studentPlan);
        await _context.SaveChangesAsync();

        // Taksitleri oluştur
        await CreateInstallmentsAsync(studentPlan.Id, paymentPlan, dto.TotalAmount, dto.StartDate, dto.FirstInstallmentAmount);

        return (await GetByIdAsync(studentPlan.Id))!;
    }

    private async Task CreateInstallmentsAsync(int studentPlanId, PaymentPlan plan, decimal totalAmount, DateTime startDate, decimal? firstInstallmentAmount)
    {
        var installments = new List<PaymentInstallment>();
        var regularInstallmentAmount = totalAmount / plan.InstallmentCount;

        for (int i = 1; i <= plan.InstallmentCount; i++)
        {
            var amount = i == 1 && firstInstallmentAmount.HasValue
                ? firstInstallmentAmount.Value
                : regularInstallmentAmount;

            var installment = new PaymentInstallment
            {
                StudentPaymentPlanId = studentPlanId,
                InstallmentNumber = i,
                Amount = Math.Round(amount, 2),
                DueDate = startDate.AddDays((i - 1) * plan.DaysBetweenInstallments),
                Status = InstallmentStatus.Pending
            };

            installments.Add(installment);
        }

        _context.Set<PaymentInstallment>().AddRange(installments);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> CancelAsync(int id, string reason)
    {
        var plan = await _context.Set<StudentPaymentPlan>().FindAsync(id);
        if (plan == null)
            return false;

        plan.Status = PaymentPlanStatus.Cancelled;
        plan.Notes = $"{plan.Notes}\nCancelled: {reason}";
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CompleteAsync(int id)
    {
        var plan = await _context.Set<StudentPaymentPlan>()
            .Include(p => p.Installments)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (plan == null)
            return false;

        plan.Status = PaymentPlanStatus.Completed;
        plan.EndDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<object> GetStatisticsAsync()
    {
        var plans = await _context.Set<StudentPaymentPlan>()
            .Include(p => p.Installments)
            .ToListAsync();

        return new
        {
            TotalPlans = plans.Count,
            ActivePlans = plans.Count(p => p.Status == PaymentPlanStatus.Active),
            CompletedPlans = plans.Count(p => p.Status == PaymentPlanStatus.Completed),
            CancelledPlans = plans.Count(p => p.Status == PaymentPlanStatus.Cancelled),
            TotalAmount = plans.Sum(p => p.TotalAmount),
            TotalPaid = plans.Sum(p => p.PaidAmount),
            TotalRemaining = plans.Sum(p => p.RemainingAmount),
            CollectionRate = plans.Sum(p => p.TotalAmount) > 0
                ? (plans.Sum(p => p.PaidAmount) / plans.Sum(p => p.TotalAmount)) * 100
                : 0
        };
    }

    public async Task<IEnumerable<StudentPaymentPlanDto>> GetOverduePlansAsync()
    {
        return await _context.Set<StudentPaymentPlan>()
            .Include(p => p.Student).ThenInclude(s => s.User)
            .Include(p => p.PaymentPlan)
            .Include(p => p.Installments)
            .Where(p => p.Status == PaymentPlanStatus.Active &&
                       p.Installments.Any(i => i.Status == InstallmentStatus.Overdue))
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    private static StudentPaymentPlanDto MapToDto(StudentPaymentPlan plan)
    {
        return new StudentPaymentPlanDto
        {
            Id = plan.Id,
            StudentId = plan.StudentId,
            StudentName = $"{plan.Student?.User?.FirstName} {plan.Student?.User?.LastName}",
            StudentNo = plan.Student?.StudentNo,
            PaymentPlanId = plan.PaymentPlanId,
            PaymentPlanName = plan.PaymentPlan?.PlanName,
            TotalAmount = plan.TotalAmount,
            PaidAmount = plan.PaidAmount,
            RemainingAmount = plan.RemainingAmount,
            StartDate = plan.StartDate,
            EndDate = plan.EndDate,
            Status = plan.Status.ToString(),
            Notes = plan.Notes,
            TotalInstallments = plan.Installments?.Count ?? 0,
            PaidInstallments = plan.Installments?.Count(i => i.Status == InstallmentStatus.Paid) ?? 0,
            OverdueInstallments = plan.Installments?.Count(i => i.Status == InstallmentStatus.Overdue) ?? 0,
            CreatedAt = plan.CreatedAt
        };
    }
}
