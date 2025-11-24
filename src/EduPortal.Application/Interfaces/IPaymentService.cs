using EduPortal.Application.DTOs.Payment;
using EduPortal.Domain.Enums;

namespace EduPortal.Application.Interfaces;

public interface IPaymentService
{
    Task<(IEnumerable<PaymentSummaryDto> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize);
    Task<PaymentDto?> GetByIdAsync(int id);
    Task<PaymentDto> CreateAsync(PaymentCreateDto dto);
    Task<PaymentDto> UpdateAsync(int id, PaymentCreateDto dto);
    Task<bool> DeleteAsync(int id);

    // Student payments
    Task<(IEnumerable<PaymentSummaryDto> Items, int TotalCount)> GetByStudentPagedAsync(int studentId, int pageNumber, int pageSize);

    // Status-based queries
    Task<(IEnumerable<PaymentSummaryDto> Items, int TotalCount)> GetByStatusPagedAsync(PaymentStatus status, int pageNumber, int pageSize);
    Task<(IEnumerable<PaymentSummaryDto> Items, int TotalCount)> GetPendingPagedAsync(int pageNumber, int pageSize);
    Task<(IEnumerable<PaymentSummaryDto> Items, int TotalCount)> GetOverduePagedAsync(int pageNumber, int pageSize);

    // Process payment
    Task<PaymentDto> ProcessPaymentAsync(int id);

    // Statistics
    Task<PaymentStatisticsDto> GetStatisticsAsync();

    // Receipt
    Task<byte[]?> GenerateReceiptAsync(int id);
}
