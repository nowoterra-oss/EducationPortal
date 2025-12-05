namespace EduPortal.Domain.Enums;

public enum HomeworkAssignmentStatus
{
    Atandi = 0,           // Ödev atandı
    Goruldu = 1,          // Öğrenci gördü
    DevamEdiyor = 2,      // Öğrenci üzerinde çalışıyor
    TeslimEdildi = 3,     // Öğrenci teslim etti
    Degerlendirildi = 4,  // Öğretmen değerlendirdi
    Gecikti = 5           // Süresi geçti
}
