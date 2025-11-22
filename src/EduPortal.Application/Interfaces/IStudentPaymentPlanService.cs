using EduPortal.Application.DTOs.PaymentPlan;

namespace EduPortal.Application.Interfaces;

public interface IStudentPaymentPlanService
{
    Task<IEnumerable<StudentPaymentPlanDto>> GetAllAsync();
    Task<StudentPaymentPlanDto?> GetByIdAsync(int id);
    Task<IEnumerable<StudentPaymentPlanDto>> GetByStudentIdAsync(int studentId);
    Task<StudentPaymentPlanDto?> GetActiveByStudentIdAsync(int studentId);
    Task<StudentPaymentPlanDto> CreateAsync(CreateStudentPaymentPlanDto dto);
    Task<bool> CancelAsync(int id, string reason);
    Task<bool> CompleteAsync(int id);

    // İstatistikler
    Task<object> GetStatisticsAsync();
    Task<IEnumerable<StudentPaymentPlanDto>> GetOverduePlansAsync();
}
