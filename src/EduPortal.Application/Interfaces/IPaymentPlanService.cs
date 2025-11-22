using EduPortal.Application.DTOs.PaymentPlan;

namespace EduPortal.Application.Interfaces;

public interface IPaymentPlanService
{
    Task<IEnumerable<PaymentPlanDto>> GetAllAsync();
    Task<IEnumerable<PaymentPlanDto>> GetActiveAsync();
    Task<PaymentPlanDto?> GetByIdAsync(int id);
    Task<PaymentPlanDto> CreateAsync(CreatePaymentPlanDto dto);
    Task<PaymentPlanDto> UpdateAsync(int id, CreatePaymentPlanDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> ActivateAsync(int id);
    Task<bool> DeactivateAsync(int id);
}
