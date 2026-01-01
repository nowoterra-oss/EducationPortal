using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Homework;

public class HomeworkAssignmentDto
{
    public int Id { get; set; }
    public int HomeworkId { get; set; }
    public string HomeworkTitle { get; set; } = string.Empty;
    public string? HomeworkDescription { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentNo { get; set; } = string.Empty;
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;

    // Ders bilgisi
    public int? CourseId { get; set; }
    public string? CourseName { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsViewed { get; set; }
    public DateTime? ViewedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public int CompletionPercentage { get; set; }

    // Öğretmenin yüklediği dosya (Homework'tan)
    public string? AttachmentUrl { get; set; }

    // Öğrenci teslimi
    public string? SubmissionText { get; set; }
    public string? SubmissionUrl { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public List<SubmissionFileDto> SubmissionFiles { get; set; } = new();

    // Öğrenci test teslimi
    public string? TestSubmissionText { get; set; }
    public string? TestSubmissionUrl { get; set; }
    public DateTime? TestSubmittedAt { get; set; }

    // Öğretmen değerlendirmesi
    public string? TeacherFeedback { get; set; }
    public int? Score { get; set; }
    public DateTime? GradedAt { get; set; }

    // Ayrı değerlendirmeler (ödev ve test için)
    public int? HomeworkScore { get; set; }
    public string? HomeworkFeedback { get; set; }
    public int? TestScore { get; set; }
    public string? TestFeedback { get; set; }

    // Ders İçeriği (Content Resources)
    public ContentResourcesDto? ContentResources { get; set; }

    // Ders Sonu Testi (Test Resources)
    public TestInfoDto? TestInfo { get; set; }

    // Flags for list view
    public bool HasContentResources { get; set; }
    public bool HasTest { get; set; }

    public bool IsOverdue => DateTime.UtcNow > DueDate && Status != "TeslimEdildi" && Status != "TestTeslimEdildi" && Status != "Degerlendirildi";
    public int DaysRemaining => (DueDate - DateTime.UtcNow).Days;
}

/// <summary>
/// Ders içeriği kaynakları
/// </summary>
public class ContentResourcesDto
{
    public List<ResourceUrlDto> Urls { get; set; } = new();
    public List<ResourceFileDto> Files { get; set; } = new();
}

/// <summary>
/// Ders sonu testi bilgileri
/// </summary>
public class TestInfoDto
{
    public DateTime? DueDate { get; set; }
    public List<ResourceUrlDto> Urls { get; set; } = new();
    public List<ResourceFileDto> Files { get; set; } = new();
}

/// <summary>
/// URL kaynağı
/// </summary>
public class ResourceUrlDto
{
    public string Url { get; set; } = string.Empty;
    public string? Title { get; set; }
}

/// <summary>
/// Dosya kaynağı
/// </summary>
public class ResourceFileDto
{
    public string Name { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public long? FileSize { get; set; }
}

public class SubmissionFileDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? ContentType { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class CreateHomeworkAssignmentDto
{
    [Required(ErrorMessage = "Öğrenci seçilmelidir")]
    public int StudentId { get; set; }

    /// <summary>
    /// Ödevi atayan öğretmen ID. Belirtilmezse token'dan alınır.
    /// </summary>
    public int? TeacherId { get; set; }

    [Required(ErrorMessage = "Başlık boş olamaz")]
    [MaxLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Açıklama en fazla 2000 karakter olabilir")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Başlangıç tarihi belirtilmelidir")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Bitiş tarihi belirtilmelidir")]
    public DateTime DueDate { get; set; }

    [MaxLength(500)]
    public string? AttachmentUrl { get; set; }

    [MaxLength(4000)]
    public string? TextContent { get; set; }

    public int? CourseId { get; set; }

    /// <summary>
    /// Yoklama kontrolünü atla (öğretmen yoklama sayfasından ödev veriyorsa true)
    /// </summary>
    public bool SkipAttendanceCheck { get; set; } = false;

    // Ders İçeriği (Content Resources)
    public List<string>? ContentUrls { get; set; }
    public List<ContentFileDto>? ContentFiles { get; set; }

    /// <summary>
    /// Seçilen ders kaynakları ID'leri (CourseResource tablosundan)
    /// </summary>
    public List<int>? CourseResourceIds { get; set; }

    // Ders Sonu Testi (Test Resources)
    public DateTime? TestDueDate { get; set; }
    public List<string>? TestUrls { get; set; }
    public List<ContentFileDto>? TestFiles { get; set; }

    /// <summary>
    /// Ders sonu testi var mı?
    /// </summary>
    public bool? HasTest { get; set; }
}

/// <summary>
/// Dosya oluşturma DTO'su
/// </summary>
public class ContentFileDto
{
    public string Name { get; set; } = string.Empty;
    public string? Base64 { get; set; } // Base64 encoded file content
    public string? Url { get; set; } // Already uploaded file URL
}

public class BulkCreateHomeworkAssignmentDto
{
    [Required(ErrorMessage = "En az bir öğrenci seçilmelidir")]
    [MinLength(1, ErrorMessage = "En az bir öğrenci seçilmelidir")]
    public List<int> StudentIds { get; set; } = new();

    /// <summary>
    /// Ödevi atayan öğretmen ID. Belirtilmezse token'dan alınır.
    /// </summary>
    public int? TeacherId { get; set; }

    [Required(ErrorMessage = "Başlık boş olamaz")]
    [MaxLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Açıklama en fazla 2000 karakter olabilir")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Başlangıç tarihi belirtilmelidir")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Bitiş tarihi belirtilmelidir")]
    public DateTime DueDate { get; set; }

    [MaxLength(500)]
    public string? AttachmentUrl { get; set; }

    [MaxLength(4000)]
    public string? TextContent { get; set; }

    public int? CourseId { get; set; }

    /// <summary>
    /// Yoklama kontrolünü atla (öğretmen yoklama sayfasından ödev veriyorsa true)
    /// </summary>
    public bool SkipAttendanceCheck { get; set; } = false;

    // Ders İçeriği (Content Resources)
    public List<string>? ContentUrls { get; set; }
    public List<ContentFileDto>? ContentFiles { get; set; }

    /// <summary>
    /// Seçilen ders kaynakları ID'leri (CourseResource tablosundan)
    /// </summary>
    public List<int>? CourseResourceIds { get; set; }

    // Ders Sonu Testi (Test Resources)
    public DateTime? TestDueDate { get; set; }
    public List<string>? TestUrls { get; set; }
    public List<ContentFileDto>? TestFiles { get; set; }

    /// <summary>
    /// Ders sonu testi var mı?
    /// </summary>
    public bool? HasTest { get; set; }
}

public class GradeHomeworkDto
{
    public int AssignmentId { get; set; }

    [Required(ErrorMessage = "Tamamlanma yüzdesi belirtilmelidir")]
    [Range(0, 100, ErrorMessage = "Tamamlanma yüzdesi 0-100 arasında olmalıdır")]
    public int CompletionPercentage { get; set; } // 10, 20, 30, ... 100

    [MaxLength(2000, ErrorMessage = "Geri bildirim en fazla 2000 karakter olabilir")]
    public string? TeacherFeedback { get; set; }

    [Range(0, 100, ErrorMessage = "Puan 0-100 arasında olmalıdır")]
    public int? Score { get; set; }

    // Ayrı değerlendirmeler (ödev ve test için)
    [Range(0, 100, ErrorMessage = "Ödev puanı 0-100 arasında olmalıdır")]
    public int? HomeworkScore { get; set; }

    [MaxLength(2000, ErrorMessage = "Ödev geri bildirimi en fazla 2000 karakter olabilir")]
    public string? HomeworkFeedback { get; set; }

    [Range(0, 100, ErrorMessage = "Test puanı 0-100 arasında olmalıdır")]
    public int? TestScore { get; set; }

    [MaxLength(2000, ErrorMessage = "Test geri bildirimi en fazla 2000 karakter olabilir")]
    public string? TestFeedback { get; set; }
}

public class StudentSummaryDto
{
    public int Id { get; set; }
    public string StudentNo { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? CourseName { get; set; }
}

public class SubmitHomeworkDto
{
    [MaxLength(4000, ErrorMessage = "Açıklama en fazla 4000 karakter olabilir")]
    public string? SubmissionText { get; set; }

    [MaxLength(500)]
    public string? SubmissionUrl { get; set; }
}

public class SubmitTestDto
{
    [MaxLength(4000, ErrorMessage = "Açıklama en fazla 4000 karakter olabilir")]
    public string? TestSubmissionText { get; set; }

    [MaxLength(500)]
    public string? TestSubmissionUrl { get; set; }
}

public class FileUploadResultDto
{
    public bool Success { get; set; }
    public string? FileUrl { get; set; }
    /// <summary>
    /// FileUrl ile aynı değer (frontend uyumluluğu için)
    /// </summary>
    public string? Url => FileUrl;
    public string? FileName { get; set; }
    public long FileSize { get; set; }
    public string? ContentType { get; set; }
    public string? ErrorMessage { get; set; }
}
