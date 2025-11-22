using EduPortal.Application.DTOs.PaymentPlan;

namespace EduPortal.Application.Interfaces;

public interface IPaymentInstallmentService
{
    Task<IEnumerable<PaymentInstallmentDto>> GetByStudentPaymentPlanAsync(int planId);
    Task<IEnumerable<PaymentInstallmentDto>> GetByStudentIdAsync(int studentId);
    Task<PaymentInstallmentDto?> GetByIdAsync(int id);
    Task<IEnumerable<PaymentInstallmentDto>> GetOverdueAsync();
    Task<IEnumerable<PaymentInstallmentDto>> GetUpcomingAsync(int days = 7);
    Task<PaymentInstallmentDto> PayInstallmentAsync(int installmentId, PayInstallmentDto dto);
    Task<object> GetStatisticsAsync();
}
