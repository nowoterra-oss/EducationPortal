using EduPortal.Application.Common;
using EduPortal.Application.DTOs.Finance;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace EduPortal.Infrastructure.Services.Finance;

public class FinanceService : IFinanceService
{
    private readonly ApplicationDbContext _context;

    public FinanceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<FinanceRecordDto>> GetAllPagedAsync(
        int pageNumber,
        int pageSize,
        FinanceType? type = null,
        FinanceCategory? category = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var query = _context.FinanceRecords
            .Include(f => f.Branch)
            .Include(f => f.RelatedStudent).ThenInclude(s => s!.User)
            .Include(f => f.RelatedTeacher).ThenInclude(t => t!.User)
            .Include(f => f.RecurringExpense)
            .Where(f => !f.IsDeleted)
            .AsQueryable();

        if (type.HasValue)
            query = query.Where(f => f.Type == type.Value);

        if (category.HasValue)
            query = query.Where(f => f.Category == category.Value);

        if (startDate.HasValue)
            query = query.Where(f => f.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(f => f.TransactionDate <= endDate.Value);

        query = query.OrderByDescending(f => f.TransactionDate);

        var totalCount = await query.CountAsync();

        var records = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = records.Select(MapToDto);

        return new PagedResult<FinanceRecordDto>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<FinanceRecordDto?> GetByIdAsync(int id)
    {
        var record = await _context.FinanceRecords
            .Include(f => f.Branch)
            .Include(f => f.RelatedStudent).ThenInclude(s => s!.User)
            .Include(f => f.RelatedTeacher).ThenInclude(t => t!.User)
            .Include(f => f.RecurringExpense)
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);

        return record != null ? MapToDto(record) : null;
    }

    public async Task<FinanceRecordDto> CreateAsync(FinanceRecordCreateDto dto)
    {
        var record = new FinanceRecord
        {
            Type = dto.Type,
            Category = dto.Category,
            Title = dto.Title,
            Description = dto.Description,
            Amount = dto.Amount,
            TransactionDate = dto.TransactionDate,
            PaymentMethod = dto.PaymentMethod,
            TransactionId = dto.TransactionId,
            ReceiptNumber = dto.ReceiptNumber ?? GenerateReceiptNumber(dto.Type),
            DocumentUrl = dto.DocumentUrl,
            BranchId = dto.BranchId,
            RelatedStudentId = dto.RelatedStudentId,
            RelatedTeacherId = dto.RelatedTeacherId,
            Notes = dto.Notes
        };

        _context.FinanceRecords.Add(record);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(record.Id))!;
    }

    public async Task<FinanceRecordDto> UpdateAsync(int id, FinanceRecordCreateDto dto)
    {
        var record = await _context.FinanceRecords.FindAsync(id);
        if (record == null || record.IsDeleted)
            throw new KeyNotFoundException("Kayıt bulunamadı.");

        record.Type = dto.Type;
        record.Category = dto.Category;
        record.Title = dto.Title;
        record.Description = dto.Description;
        record.Amount = dto.Amount;
        record.TransactionDate = dto.TransactionDate;
        record.PaymentMethod = dto.PaymentMethod;
        record.TransactionId = dto.TransactionId;
        record.DocumentUrl = dto.DocumentUrl;
        record.BranchId = dto.BranchId;
        record.RelatedStudentId = dto.RelatedStudentId;
        record.RelatedTeacherId = dto.RelatedTeacherId;
        record.Notes = dto.Notes;
        record.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(id))!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var record = await _context.FinanceRecords.FindAsync(id);
        if (record == null || record.IsDeleted)
            return false;

        record.IsDeleted = true;
        record.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<FinanceRecordDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, FinanceType? type = null)
    {
        var query = _context.FinanceRecords
            .Include(f => f.Branch)
            .Include(f => f.RelatedStudent).ThenInclude(s => s!.User)
            .Include(f => f.RelatedTeacher).ThenInclude(t => t!.User)
            .Where(f => f.TransactionDate >= startDate && f.TransactionDate <= endDate && !f.IsDeleted);

        if (type.HasValue)
            query = query.Where(f => f.Type == type.Value);

        var records = await query.OrderByDescending(f => f.TransactionDate).ToListAsync();

        return records.Select(MapToDto).ToList();
    }

    public async Task<FinanceStatisticsDto> GetStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        startDate ??= monthStart.AddMonths(-11);
        endDate ??= monthEnd;

        var records = await _context.FinanceRecords
            .Where(f => !f.IsDeleted && f.TransactionDate >= startDate && f.TransactionDate <= endDate)
            .ToListAsync();

        var incomeRecords = records.Where(r => r.Type == FinanceType.Gelir).ToList();
        var expenseRecords = records.Where(r => r.Type == FinanceType.Gider).ToList();

        var monthlyRecords = records.Where(r => r.TransactionDate >= monthStart && r.TransactionDate <= monthEnd).ToList();

        var pendingPayments = await _context.PaymentInstallments
            .Where(i => i.Status == InstallmentStatus.Pending && !i.StudentPaymentPlan.IsDeleted)
            .SumAsync(i => i.Amount - i.PaidAmount);

        var overduePayments = await _context.PaymentInstallments
            .Where(i => i.Status == InstallmentStatus.Overdue && !i.StudentPaymentPlan.IsDeleted)
            .SumAsync(i => i.Amount - i.PaidAmount);

        var pendingSalaries = await _context.TeacherSalaries
            .Where(s => (s.Status == SalaryStatus.Bekliyor || s.Status == SalaryStatus.Gecikti) && !s.IsDeleted)
            .SumAsync(s => s.BaseSalary + s.Bonus - s.Deduction);

        var paidSalariesThisMonth = await _context.TeacherSalaries
            .Where(s => s.Status == SalaryStatus.Odendi && s.PaidDate >= monthStart && s.PaidDate <= monthEnd && !s.IsDeleted)
            .SumAsync(s => s.BaseSalary + s.Bonus - s.Deduction);

        var totalIncome = incomeRecords.Sum(r => r.Amount);
        var totalExpense = expenseRecords.Sum(r => r.Amount);

        var stats = new FinanceStatisticsDto
        {
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            NetBalance = totalIncome - totalExpense,

            MonthlyIncome = monthlyRecords.Where(r => r.Type == FinanceType.Gelir).Sum(r => r.Amount),
            MonthlyExpense = monthlyRecords.Where(r => r.Type == FinanceType.Gider).Sum(r => r.Amount),
            MonthlyNetBalance = monthlyRecords.Where(r => r.Type == FinanceType.Gelir).Sum(r => r.Amount)
                - monthlyRecords.Where(r => r.Type == FinanceType.Gider).Sum(r => r.Amount),

            PendingStudentPayments = pendingPayments,
            OverdueStudentPayments = overduePayments,
            PendingSalaries = pendingSalaries,
            PaidSalaries = paidSalariesThisMonth,

            IncomeByCategory = GetCategoryBreakdown(incomeRecords, totalIncome),
            ExpenseByCategory = GetCategoryBreakdown(expenseRecords, totalExpense),
            MonthlyTrend = GetMonthlyTrend(records, startDate.Value, endDate.Value)
        };

        return stats;
    }

    public async Task<PagedResult<RecurringExpenseDto>> GetRecurringExpensesPagedAsync(int pageNumber, int pageSize, bool? isActive = null)
    {
        var query = _context.RecurringExpenses
            .Include(r => r.Branch)
            .Where(r => !r.IsDeleted)
            .AsQueryable();

        if (isActive.HasValue)
            query = query.Where(r => r.IsActive == isActive.Value);

        query = query.OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync();

        var records = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = records.Select(MapToRecurringDto);

        return new PagedResult<RecurringExpenseDto>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<RecurringExpenseDto?> GetRecurringExpenseByIdAsync(int id)
    {
        var record = await _context.RecurringExpenses
            .Include(r => r.Branch)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        return record != null ? MapToRecurringDto(record) : null;
    }

    public async Task<RecurringExpenseDto> CreateRecurringExpenseAsync(RecurringExpenseCreateDto dto)
    {
        var record = new RecurringExpense
        {
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            Amount = dto.Amount,
            RecurrenceType = dto.RecurrenceType,
            RecurrenceDay = dto.RecurrenceDay,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            NextDueDate = CalculateNextDueDate(dto.StartDate, dto.RecurrenceType, dto.RecurrenceDay),
            IsActive = true,
            BranchId = dto.BranchId,
            Notes = dto.Notes
        };

        _context.RecurringExpenses.Add(record);
        await _context.SaveChangesAsync();

        return (await GetRecurringExpenseByIdAsync(record.Id))!;
    }

    public async Task<RecurringExpenseDto> UpdateRecurringExpenseAsync(int id, RecurringExpenseCreateDto dto)
    {
        var record = await _context.RecurringExpenses.FindAsync(id);
        if (record == null || record.IsDeleted)
            throw new KeyNotFoundException("Kayıt bulunamadı.");

        record.Title = dto.Title;
        record.Description = dto.Description;
        record.Category = dto.Category;
        record.Amount = dto.Amount;
        record.RecurrenceType = dto.RecurrenceType;
        record.RecurrenceDay = dto.RecurrenceDay;
        record.StartDate = dto.StartDate;
        record.EndDate = dto.EndDate;
        record.BranchId = dto.BranchId;
        record.Notes = dto.Notes;
        record.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (await GetRecurringExpenseByIdAsync(id))!;
    }

    public async Task<bool> DeleteRecurringExpenseAsync(int id)
    {
        var record = await _context.RecurringExpenses.FindAsync(id);
        if (record == null || record.IsDeleted)
            return false;

        record.IsDeleted = true;
        record.IsActive = false;
        record.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<RecurringExpenseDto> ToggleRecurringExpenseAsync(int id)
    {
        var record = await _context.RecurringExpenses.FindAsync(id);
        if (record == null || record.IsDeleted)
            throw new KeyNotFoundException("Kayıt bulunamadı.");

        record.IsActive = !record.IsActive;
        record.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (await GetRecurringExpenseByIdAsync(id))!;
    }

    public async Task ProcessRecurringExpensesAsync()
    {
        var today = DateTime.Today;
        var recurringExpenses = await _context.RecurringExpenses
            .Where(r => r.IsActive && !r.IsDeleted && r.NextDueDate <= today)
            .Where(r => r.EndDate == null || r.EndDate >= today)
            .ToListAsync();

        foreach (var expense in recurringExpenses)
        {
            var financeRecord = new FinanceRecord
            {
                Type = FinanceType.Gider,
                Category = expense.Category,
                Title = expense.Title,
                Description = $"Otomatik düzenli gider: {expense.Description}",
                Amount = expense.Amount,
                TransactionDate = expense.NextDueDate ?? today,
                BranchId = expense.BranchId,
                RecurringExpenseId = expense.Id,
                ReceiptNumber = GenerateReceiptNumber(FinanceType.Gider)
            };

            _context.FinanceRecords.Add(financeRecord);

            expense.LastProcessedDate = today;
            expense.NextDueDate = CalculateNextDueDate(today, expense.RecurrenceType, expense.RecurrenceDay);

            if (expense.EndDate.HasValue && expense.NextDueDate > expense.EndDate)
            {
                expense.IsActive = false;
            }
        }

        await _context.SaveChangesAsync();
    }

    private static DateTime CalculateNextDueDate(DateTime fromDate, RecurrenceType type, int recurrenceDay)
    {
        return type switch
        {
            RecurrenceType.Gunluk => fromDate.AddDays(1),
            RecurrenceType.Haftalik => fromDate.AddDays(7),
            RecurrenceType.Aylik => new DateTime(fromDate.Year, fromDate.Month, Math.Min(recurrenceDay, DateTime.DaysInMonth(fromDate.Year, fromDate.Month))).AddMonths(1),
            RecurrenceType.Yillik => fromDate.AddYears(1),
            _ => fromDate.AddMonths(1)
        };
    }

    private static string GenerateReceiptNumber(FinanceType type)
    {
        var prefix = type == FinanceType.Gelir ? "INC" : "EXP";
        return $"{prefix}-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }

    private static List<CategoryBreakdownDto> GetCategoryBreakdown(List<FinanceRecord> records, decimal total)
    {
        if (total == 0) return new List<CategoryBreakdownDto>();

        return records
            .GroupBy(r => r.Category)
            .Select(g => new CategoryBreakdownDto
            {
                Category = g.Key.ToString(),
                Amount = g.Sum(r => r.Amount),
                Percentage = Math.Round(g.Sum(r => r.Amount) / total * 100, 2)
            })
            .OrderByDescending(c => c.Amount)
            .ToList();
    }

    private static List<MonthlyTrendDto> GetMonthlyTrend(List<FinanceRecord> records, DateTime startDate, DateTime endDate)
    {
        var culture = new CultureInfo("tr-TR");
        var result = new List<MonthlyTrendDto>();

        var current = new DateTime(startDate.Year, startDate.Month, 1);
        var end = new DateTime(endDate.Year, endDate.Month, 1);

        while (current <= end)
        {
            var monthRecords = records.Where(r =>
                r.TransactionDate.Year == current.Year && r.TransactionDate.Month == current.Month).ToList();

            var income = monthRecords.Where(r => r.Type == FinanceType.Gelir).Sum(r => r.Amount);
            var expense = monthRecords.Where(r => r.Type == FinanceType.Gider).Sum(r => r.Amount);

            result.Add(new MonthlyTrendDto
            {
                Year = current.Year,
                Month = current.Month,
                MonthName = culture.DateTimeFormat.GetMonthName(current.Month),
                Income = income,
                Expense = expense,
                Net = income - expense
            });

            current = current.AddMonths(1);
        }

        return result;
    }

    private static FinanceRecordDto MapToDto(FinanceRecord record)
    {
        return new FinanceRecordDto
        {
            Id = record.Id,
            Type = record.Type,
            Category = record.Category,
            Title = record.Title,
            Description = record.Description,
            Amount = record.Amount,
            TransactionDate = record.TransactionDate,
            PaymentMethod = record.PaymentMethod,
            TransactionId = record.TransactionId,
            ReceiptNumber = record.ReceiptNumber,
            DocumentUrl = record.DocumentUrl,
            BranchId = record.BranchId,
            BranchName = record.Branch?.BranchName,
            RelatedStudentId = record.RelatedStudentId,
            RelatedStudentName = record.RelatedStudent?.User != null
                ? $"{record.RelatedStudent.User.FirstName} {record.RelatedStudent.User.LastName}"
                : null,
            RelatedTeacherId = record.RelatedTeacherId,
            RelatedTeacherName = record.RelatedTeacher?.User != null
                ? $"{record.RelatedTeacher.User.FirstName} {record.RelatedTeacher.User.LastName}"
                : null,
            RecurringExpenseId = record.RecurringExpenseId,
            RecurringExpenseTitle = record.RecurringExpense?.Title,
            Notes = record.Notes,
            CreatedAt = record.CreatedAt
        };
    }

    private static RecurringExpenseDto MapToRecurringDto(RecurringExpense record)
    {
        return new RecurringExpenseDto
        {
            Id = record.Id,
            Title = record.Title,
            Description = record.Description,
            Category = record.Category,
            Amount = record.Amount,
            RecurrenceType = record.RecurrenceType,
            RecurrenceDay = record.RecurrenceDay,
            StartDate = record.StartDate,
            EndDate = record.EndDate,
            LastProcessedDate = record.LastProcessedDate,
            NextDueDate = record.NextDueDate,
            IsActive = record.IsActive,
            BranchId = record.BranchId,
            BranchName = record.Branch?.BranchName,
            Notes = record.Notes,
            CreatedAt = record.CreatedAt
        };
    }
}
