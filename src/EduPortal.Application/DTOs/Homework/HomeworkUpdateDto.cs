using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Homework;

public class HomeworkUpdateDto
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "Ba_l1k zorunludur")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Son teslim tarihi zorunludur")]
    public DateTime DueDate { get; set; }

    [Range(0, 1000, ErrorMessage = "Maksimum puan 0-1000 aras1nda olmal1d1r")]
    public int? MaxScore { get; set; }

    [StringLength(500)]
    public string? AttachmentUrl { get; set; }
}
