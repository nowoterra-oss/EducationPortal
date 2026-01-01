namespace EduPortal.Application.DTOs.Homework;

public class HomeworkProgressDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Progress bar için
    public DateTime AssignedDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public int ProgressPercentage { get; set; }
    public string Status { get; set; } = string.Empty;

    // Müfredat bağlantısı
    public int? CurriculumId { get; set; }
    public string? CurriculumTopicName { get; set; }

    // Ekler
    public List<HomeworkAttachmentDto> Attachments { get; set; } = new();

    // Hesaplanan alanlar
    public int TotalDays => (DueDate - AssignedDate).Days;
    public int DaysRemaining => Math.Max(0, (DueDate - DateTime.UtcNow).Days);
    public int DaysElapsed => Math.Max(0, (DateTime.UtcNow - AssignedDate).Days);
    public double ProgressTimePercentage => TotalDays > 0
        ? Math.Min(100, (double)DaysElapsed / TotalDays * 100)
        : 100;
    public bool IsOverdue => DateTime.UtcNow > DueDate && SubmittedAt == null;
}

public class HomeworkAttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? MimeType { get; set; }
    public long FileSize { get; set; }
    public bool IsFromCourseResource { get; set; }
    public int? CourseResourceId { get; set; }
    public string? CourseResourceTitle { get; set; }
}
