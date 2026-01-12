using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.Finance;

public class TeacherSalaryDto
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string? TeacherEmail { get; set; }

    public decimal BaseSalary { get; set; }
    public decimal Bonus { get; set; }
    public decimal Deduction { get; set; }
    public decimal NetSalary { get; set; }

    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;

    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }

    public SalaryStatus Status { get; set; }
    public string StatusName => Status.ToString();

    public PaymentMethod? PaymentMethod { get; set; }
    public string? PaymentMethodName => PaymentMethod?.ToString();

    public string? TransactionId { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
}
