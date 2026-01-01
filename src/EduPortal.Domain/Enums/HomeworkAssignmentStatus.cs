namespace EduPortal.Domain.Enums;

public enum HomeworkAssignmentStatus
{
    Atandi = 0,           // Ödev atandı
    Goruldu = 1,          // Öğrenci gördü
    DevamEdiyor = 2,      // Öğrenci üzerinde çalışıyor
    TeslimEdildi = 3,     // Öğrenci teslim etti
    TestTeslimEdildi = 4, // Öğrenci testi teslim etti (test varsa)
    Degerlendirildi = 5,  // Öğretmen değerlendirdi
    Gecikti = 6           // Süresi geçti
}
