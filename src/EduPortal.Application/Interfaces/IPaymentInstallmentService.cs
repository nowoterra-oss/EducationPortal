using EduPortal.Application.Common;
using EduPortal.Application.DTOs.PaymentPlan;
using Microsoft.AspNetCore.Http;

namespace EduPortal.Application.Interfaces;

public interface IPaymentInstallmentService
{
    Task<IEnumerable<PaymentInstallmentDto>> GetByStudentPaymentPlanAsync(int planId);
    Task<IEnumerable<PaymentInstallmentDto>> GetByStudentIdAsync(int studentId);
    Task<IEnumerable<PaymentInstallmentDto>> GetByParentIdAsync(int parentId);
    Task<PaymentInstallmentDto?> GetByIdAsync(int id);
    Task<IEnumerable<PaymentInstallmentDto>> GetOverdueAsync();
    Task<IEnumerable<PaymentInstallmentDto>> GetUpcomingAsync(int days = 7);
    Task<PaymentInstallmentDto> PayInstallmentAsync(int installmentId, PayInstallmentDto dto);
    Task<object> GetStatisticsAsync();

    // Dekont işlemleri
    Task<PaymentInstallmentDto> UploadReceiptAsync(int installmentId, IFormFile file, string? notes, int studentId);
    Task<(byte[] FileContent, string ContentType, string FileName)?> GetReceiptAsync(int installmentId);
    Task<PagedResult<PaymentInstallmentDto>> GetPendingApprovalAsync(int pageNumber, int pageSize);
    Task<PaymentInstallmentDto> ApproveInstallmentAsync(int installmentId, string approvedByUserId, string? notes);
    Task<PaymentInstallmentDto> RejectInstallmentAsync(int installmentId, string reason);
    Task<PaymentInstallmentDto> MarkAsCashPaymentAsync(int installmentId, string approvedByUserId, string? notes, string? paymentMethod);

    // Debug
    Task<object> GetDebugStatusAsync();
}
