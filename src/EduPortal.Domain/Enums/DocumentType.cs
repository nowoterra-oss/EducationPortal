namespace EduPortal.Domain.Enums;

public enum DocumentType
{
    Transcript = 0,              // Transkript
    DiplomaHighSchool = 1,       // Lise Diploması
    DiplomaUniversity = 2,       // Üniversite Diploması
    MotivationLetter = 3,        // Motivasyon Mektubu
    ReferenceLetter = 4,         // Referans Mektubu
    CV = 5,                      // CV/Özgeçmiş
    LanguageTestScore = 6,       // Dil Sınavı Sonucu (TOEFL, IELTS, etc.)
    StandardizedTest = 7,        // Standart Test (SAT, GRE, GMAT, etc.)
    Passport = 8,                // Pasaport
    FinancialProof = 9,          // Mali Kanıt
    Portfolio = 10,              // Portfolyo
    Essay = 11,                  // Deneme/Kompozisyon
    SOP = 12,                    // Statement of Purpose
    Other = 13                   // Diğer
}
