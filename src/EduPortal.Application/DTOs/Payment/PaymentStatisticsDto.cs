namespace EduPortal.Application.DTOs.Payment;

public class PaymentStatisticsDto
{
    public int TotalPayments { get; set; }
    public int PendingPayments { get; set; }
    public int CompletedPayments { get; set; }
    public int OverduePayments { get; set; }
    public int CancelledPayments { get; set; }

    public decimal TotalAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal TotalPendingAmount { get; set; }
    public decimal TotalOverdueAmount { get; set; }

    // Ödeme yöntemi dağılımı
    public decimal CashAmount { get; set; }
    public decimal CreditCardAmount { get; set; }
    public decimal TransferAmount { get; set; }
    public decimal OtherAmount { get; set; }

    // Zaman bazlı istatistikler
    public decimal TodayAmount { get; set; }
    public decimal ThisWeekAmount { get; set; }
    public decimal ThisMonthAmount { get; set; }

    public int TodayPaymentCount { get; set; }
    public int ThisWeekPaymentCount { get; set; }
    public int ThisMonthPaymentCount { get; set; }
}
