using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Exam;

public class ExamResultCreateDto
{
    [Required(ErrorMessage = "S1nav seçimi zorunludur")]
    public int ExamId { get; set; }

    [Required(ErrorMessage = "Örenci seçimi zorunludur")]
    public int StudentId { get; set; }

    [Required(ErrorMessage = "Puan zorunludur")]
    [Range(0, 10000, ErrorMessage = "Puan 0-10000 aras1nda olmal1d1r")]
    public decimal Score { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
