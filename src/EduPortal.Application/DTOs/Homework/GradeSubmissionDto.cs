using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Homework;

public class GradeSubmissionDto
{
    [Required]
    public int SubmissionId { get; set; }

    [Required(ErrorMessage = "Puan zorunludur")]
    [Range(0, 100, ErrorMessage = "Puan 0-100 aras1nda olmal1d1r")]
    public decimal Score { get; set; }

    [StringLength(2000)]
    public string? TeacherFeedback { get; set; }
}
