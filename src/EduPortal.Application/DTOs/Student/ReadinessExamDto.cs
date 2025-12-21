using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Student;

public class ReadinessExamDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public DateTime? ExamDate { get; set; }
    public string? Score { get; set; }
    public string? Notes { get; set; }
}

public class ReadinessExamCreateDto
{
    [Required(ErrorMessage = "Sinav adi zorunludur")]
    [MaxLength(200, ErrorMessage = "Sinav adi en fazla 200 karakter olabilir")]
    public string ExamName { get; set; } = string.Empty;

    public DateTime? ExamDate { get; set; }

    [MaxLength(50, ErrorMessage = "Puan en fazla 50 karakter olabilir")]
    public string? Score { get; set; }

    [MaxLength(1000, ErrorMessage = "Notlar en fazla 1000 karakter olabilir")]
    public string? Notes { get; set; }
}
