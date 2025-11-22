using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Exam;

public class InternalExamCreateDto
{
    [Required(ErrorMessage = "Ders seçimi zorunludur")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "S1nav tipi zorunludur")]
    [StringLength(50)]
    public string ExamType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ba_l1k zorunludur")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "S1nav tarihi zorunludur")]
    public DateTime ExamDate { get; set; }

    [Range(1, 300, ErrorMessage = "Süre 1-300 dakika aras1nda olmal1d1r")]
    public int? Duration { get; set; }

    [Required(ErrorMessage = "Maksimum puan zorunludur")]
    [Range(1, 1000, ErrorMessage = "Maksimum puan 1-1000 aras1nda olmal1d1r")]
    public int MaxScore { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }
}
