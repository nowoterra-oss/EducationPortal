using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Homework;

/// <summary>
/// Taslak ödev response DTO
/// </summary>
public class HomeworkDraftDto
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string LessonId { get; set; } = string.Empty;
    public int? CourseId { get; set; }
    public string? CourseName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? TestDueDate { get; set; }
    public List<DraftStudentDto> Students { get; set; } = new();
    public List<string>? ContentUrls { get; set; }
    public List<DraftFileDto>? ContentFiles { get; set; }
    public List<int>? CourseResourceIds { get; set; }
    public List<string>? TestUrls { get; set; }
    public List<DraftFileDto>? TestFiles { get; set; }
    public bool HasTest { get; set; }
    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Taslaktaki öğrenci bilgisi
/// </summary>
public class DraftStudentDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? StudentNo { get; set; }
    public string Status { get; set; } = string.Empty;  // Yoklama durumu: present, absent, late, excused
    public int Performance { get; set; }  // 1-5 arası performans puanı
    public string? Notes { get; set; }
}

/// <summary>
/// Taslak dosya bilgisi
/// </summary>
public class DraftFileDto
{
    public string Name { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public long? FileSize { get; set; }
}

/// <summary>
/// Taslak oluşturma/güncelleme DTO
/// </summary>
public class CreateHomeworkDraftDto
{
    [Required(ErrorMessage = "LessonId zorunludur")]
    [MaxLength(100)]
    public string LessonId { get; set; } = string.Empty;

    public int? CourseId { get; set; }

    [Required(ErrorMessage = "Başlık zorunludur")]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Bitiş tarihi zorunludur")]
    public DateTime DueDate { get; set; }

    public DateTime? TestDueDate { get; set; }

    /// <summary>
    /// Ders sonu testi var mı?
    /// </summary>
    public bool? HasTest { get; set; }

    [Required(ErrorMessage = "En az bir öğrenci seçilmelidir")]
    public List<DraftStudentDto> Students { get; set; } = new();

    public List<string>? ContentUrls { get; set; }
    public List<int>? CourseResourceIds { get; set; }
    public List<string>? TestUrls { get; set; }
}

/// <summary>
/// Taslak güncelleme DTO (partial update)
/// </summary>
public class UpdateHomeworkDraftDto
{
    [MaxLength(500)]
    public string? Title { get; set; }

    [MaxLength(4000)]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }
    public DateTime? TestDueDate { get; set; }
    public bool? HasTest { get; set; }
    public List<DraftStudentDto>? Students { get; set; }
    public List<string>? ContentUrls { get; set; }
    public List<int>? CourseResourceIds { get; set; }
    public List<string>? TestUrls { get; set; }
}

/// <summary>
/// Taslak gönderme sonucu
/// </summary>
public class SendDraftResultDto
{
    public int TotalStudents { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<string>? Errors { get; set; }
    public List<int>? CreatedAssignmentIds { get; set; }
}
