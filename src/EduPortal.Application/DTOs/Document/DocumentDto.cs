using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.Document;

public class DocumentDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public string DocumentTypeName => GetDocumentTypeName(DocumentType);
    public string Title { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public string DocumentUrl { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; }
    public string StatusName => GetStatusName(Status);
    public string? ReviewNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    private static string GetDocumentTypeName(DocumentType type) => type switch
    {
        DocumentType.Transcript => "Transkript",
        DocumentType.DiplomaHighSchool => "Lise Diploması",
        DocumentType.DiplomaUniversity => "Üniversite Diploması",
        DocumentType.MotivationLetter => "Motivasyon Mektubu",
        DocumentType.ReferenceLetter => "Referans Mektubu",
        DocumentType.CV => "CV/Özgeçmiş",
        DocumentType.LanguageTestScore => "Dil Sınavı Sonucu",
        DocumentType.StandardizedTest => "Standart Test Sonucu",
        DocumentType.Passport => "Pasaport",
        DocumentType.FinancialProof => "Mali Kanıt",
        DocumentType.Portfolio => "Portfolyo",
        DocumentType.Essay => "Deneme/Kompozisyon",
        DocumentType.SOP => "Statement of Purpose",
        DocumentType.Other => "Diğer",
        _ => type.ToString()
    };

    private static string GetStatusName(DocumentStatus status) => status switch
    {
        DocumentStatus.NotStarted => "Başlanmadı",
        DocumentStatus.InProgress => "Devam Ediyor",
        DocumentStatus.Completed => "Tamamlandı",
        DocumentStatus.Submitted => "Gönderildi",
        DocumentStatus.Approved => "Onaylandı",
        DocumentStatus.Rejected => "Reddedildi",
        _ => status.ToString()
    };
}
