using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Homework;

public class HomeworkCreateDto
{
    [Required(ErrorMessage = "Ders seçimi zorunludur")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Başlık zorunludur")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Atanma tarihi zorunludur")]
    public DateTime AssignedDate { get; set; }

    [Required(ErrorMessage = "Son teslim tarihi zorunludur")]
    public DateTime DueDate { get; set; }

    [Range(0, 1000, ErrorMessage = "Maksimum puan 0-1000 arasında olmalıdır")]
    public int? MaxScore { get; set; }

    [StringLength(500)]
    public string? AttachmentUrl { get; set; }
}
