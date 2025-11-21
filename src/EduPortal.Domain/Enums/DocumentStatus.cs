namespace EduPortal.Domain.Enums;

public enum DocumentStatus
{
    NotStarted = 0,      // Başlanmadı
    InProgress = 1,      // Devam Ediyor
    Completed = 2,       // Tamamlandı
    Submitted = 3,       // Gönderildi
    Approved = 4,        // Onaylandı
    Rejected = 5         // Reddedildi
}
