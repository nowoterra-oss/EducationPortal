namespace EduPortal.Domain.Enums;

public enum InstallmentStatus
{
    Pending = 0,          // Bekliyor
    Paid = 1,             // Ödendi
    Overdue = 2,          // Vadesi geçti
    PartiallyPaid = 3,    // Kısmi ödendi
    Cancelled = 4,        // İptal edildi
    AwaitingApproval = 5, // Dekont yüklendi, onay bekliyor
    Rejected = 6          // Reddedildi
}
