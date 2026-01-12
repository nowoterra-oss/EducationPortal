using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Finance;
using EduPortal.Domain.Enums;

namespace EduPortal.Application.Interfaces;

public interface IFinanceService
{
    Task<PagedResult<FinanceRecordDto>> GetAllPagedAsync(
        int pageNumber,
        int pageSize,
        FinanceType? type = null,
        FinanceCategory? category = null,
        DateTime? startDate = null,
        DateTime? endDate = null);

    Task<FinanceRecordDto?> GetByIdAsync(int id);

    Task<FinanceRecordDto> CreateAsync(FinanceRecordCreateDto dto);

    Task<FinanceRecordDto> UpdateAsync(int id, FinanceRecordCreateDto dto);

    Task<bool> DeleteAsync(int id);

    Task<List<FinanceRecordDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, FinanceType? type = null);

    Task<FinanceStatisticsDto> GetStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);

    Task<PagedResult<RecurringExpenseDto>> GetRecurringExpensesPagedAsync(int pageNumber, int pageSize, bool? isActive = null);

    Task<RecurringExpenseDto?> GetRecurringExpenseByIdAsync(int id);

    Task<RecurringExpenseDto> CreateRecurringExpenseAsync(RecurringExpenseCreateDto dto);

    Task<RecurringExpenseDto> UpdateRecurringExpenseAsync(int id, RecurringExpenseCreateDto dto);

    Task<bool> DeleteRecurringExpenseAsync(int id);

    Task<RecurringExpenseDto> ToggleRecurringExpenseAsync(int id);

    Task ProcessRecurringExpensesAsync();
}
