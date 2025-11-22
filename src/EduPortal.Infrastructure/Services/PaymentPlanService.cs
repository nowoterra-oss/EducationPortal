using EduPortal.Application.DTOs.PaymentPlan;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Services;

public class PaymentPlanService : IPaymentPlanService
{
    private readonly ApplicationDbContext _context;

    public PaymentPlanService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PaymentPlanDto>> GetAllAsync()
    {
        return await _context.PaymentPlans
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<IEnumerable<PaymentPlanDto>> GetActiveAsync()
    {
        return await _context.PaymentPlans
            .Where(p => p.IsActive)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<PaymentPlanDto?> GetByIdAsync(int id)
    {
        var plan = await _context.PaymentPlans.FindAsync(id);
        return plan != null ? MapToDto(plan) : null;
    }

    public async Task<PaymentPlanDto> CreateAsync(CreatePaymentPlanDto dto)
    {
        var plan = new PaymentPlan
        {
            PlanName = dto.PlanName,
            Description = dto.Description,
            InstallmentCount = dto.InstallmentCount,
            DaysBetweenInstallments = dto.DaysBetweenInstallments,
            DownPaymentDiscount = dto.DownPaymentDiscount,
            Notes = dto.Notes,
            IsActive = true
        };

        _context.PaymentPlans.Add(plan);
        await _context.SaveChangesAsync();

        return MapToDto(plan);
    }

    public async Task<PaymentPlanDto> UpdateAsync(int id, CreatePaymentPlanDto dto)
    {
        var plan = await _context.PaymentPlans.FindAsync(id);
        if (plan == null)
            throw new Exception($"Payment plan with ID {id} not found");

        plan.PlanName = dto.PlanName;
        plan.Description = dto.Description;
        plan.InstallmentCount = dto.InstallmentCount;
        plan.DaysBetweenInstallments = dto.DaysBetweenInstallments;
        plan.DownPaymentDiscount = dto.DownPaymentDiscount;
        plan.Notes = dto.Notes;

        await _context.SaveChangesAsync();
        return MapToDto(plan);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var plan = await _context.PaymentPlans.FindAsync(id);
        if (plan == null)
            return false;

        _context.PaymentPlans.Remove(plan);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActivateAsync(int id)
    {
        var plan = await _context.PaymentPlans.FindAsync(id);
        if (plan == null)
            return false;

        plan.IsActive = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var plan = await _context.PaymentPlans.FindAsync(id);
        if (plan == null)
            return false;

        plan.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    private static PaymentPlanDto MapToDto(PaymentPlan plan)
    {
        return new PaymentPlanDto
        {
            Id = plan.Id,
            PlanName = plan.PlanName,
            Description = plan.Description,
            InstallmentCount = plan.InstallmentCount,
            DaysBetweenInstallments = plan.DaysBetweenInstallments,
            DownPaymentDiscount = plan.DownPaymentDiscount,
            IsActive = plan.IsActive,
            Notes = plan.Notes,
            TotalUsageCount = plan.StudentPaymentPlans?.Count ?? 0,
            CreatedAt = plan.CreatedAt
        };
    }
}
