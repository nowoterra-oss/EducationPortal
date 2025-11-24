using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.Exam;

public class InternationalExamDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;

    public ExamType ExamType { get; set; }
    public string ExamTypeName => ExamType.ToString();
    public string ExamName { get; set; } = string.Empty;

    public int? Grade { get; set; }
    public string AcademicYear { get; set; } = string.Empty;

    public string Score { get; set; } = string.Empty;
    public string? MaxScore { get; set; }

    public DateTime? ApplicationStartDate { get; set; }
    public DateTime? ApplicationEndDate { get; set; }
    public DateTime ExamDate { get; set; }
    public DateTime? ResultDate { get; set; }

    public string? CertificateUrl { get; set; }

    public DateTime CreatedAt { get; set; }
}
