namespace EduPortal.Application.DTOs.PaymentPlan;

public class PaymentInstallmentDto
{
    public int Id { get; set; }
    public int StudentPaymentPlanId { get; set; }
    public int StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? StudentNo { get; set; }
    public int InstallmentNumber { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount => Amount - PaidAmount;
    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? PaymentId { get; set; }
    public string? Notes { get; set; }
    public int DaysOverdue { get; set; }
    public bool IsOverdue => Status == "Overdue";
    public bool IsUpcoming => DueDate <= DateTime.Now.AddDays(7) && Status == "Pending";

    // Dekont alanları
    public string? ReceiptPath { get; set; }
    public DateTime? ReceiptUploadDate { get; set; }
    public string? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string? RejectionReason { get; set; }
    public string? PaymentMethod { get; set; }
}

public class UploadReceiptDto
{
    public string? Notes { get; set; }
}

public class ApproveInstallmentDto
{
    public string? Notes { get; set; }
}

public class RejectInstallmentDto
{
    public required string Reason { get; set; }
}

public class CashPaymentDto
{
    public string? Notes { get; set; }
    public string? PaymentMethod { get; set; } // cash, bank_transfer, pos, other
}
