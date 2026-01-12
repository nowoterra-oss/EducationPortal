namespace EduPortal.Application.DTOs.Finance;

public class FinanceStatisticsDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetBalance { get; set; }

    public decimal MonthlyIncome { get; set; }
    public decimal MonthlyExpense { get; set; }
    public decimal MonthlyNetBalance { get; set; }

    public decimal PendingStudentPayments { get; set; }
    public decimal OverdueStudentPayments { get; set; }

    public decimal PendingSalaries { get; set; }
    public decimal PaidSalaries { get; set; }

    public List<CategoryBreakdownDto> IncomeByCategory { get; set; } = new();
    public List<CategoryBreakdownDto> ExpenseByCategory { get; set; } = new();
    public List<MonthlyTrendDto> MonthlyTrend { get; set; } = new();
}

public class CategoryBreakdownDto
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}

public class MonthlyTrendDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Net { get; set; }
}
