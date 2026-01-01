namespace EduPortal.Application.DTOs.Course;

public class StudentCurriculumProgressDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int CurriculumId { get; set; }
    public string TopicName { get; set; } = string.Empty;
    public int TopicOrder { get; set; }

    public bool IsTopicCompleted { get; set; }
    public bool AreHomeworksCompleted { get; set; }
    public bool IsExamUnlocked { get; set; }
    public bool IsExamCompleted { get; set; }
    public int? ExamScore { get; set; }
    public bool IsApprovedByTeacher { get; set; }

    // Detaylar
    public int TotalHomeworks { get; set; }
    public int CompletedHomeworks { get; set; }
    public double HomeworkCompletionPercentage => TotalHomeworks > 0
        ? (double)CompletedHomeworks / TotalHomeworks * 100
        : 0;

    // Sınav dosyası
    public bool HasExam { get; set; }
    public string? ExamFileName { get; set; }
    public string? ExamDownloadUrl { get; set; }
}
