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

    // Öğretmen değerlendirmesi
    public string? TeacherFeedback { get; set; }
    public int? Score { get; set; }
    public DateTime? GradedAt { get; set; }

    public bool IsOverdue => DateTime.UtcNow > DueDate && Status != "TeslimEdildi" && Status != "Degerlendirildi";
    public int DaysRemaining => (DueDate - DateTime.UtcNow).Days;
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
}

public class BulkCreateHomeworkAssignmentDto
{
    [Required(ErrorMessage = "En az bir öğrenci seçilmelidir")]
    [MinLength(1, ErrorMessage = "En az bir öğrenci seçilmelidir")]
    public List<int> StudentIds { get; set; } = new();

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

public class FileUploadResultDto
{
    public bool Success { get; set; }
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
    public long FileSize { get; set; }
    public string? ContentType { get; set; }
    public string? ErrorMessage { get; set; }
}
