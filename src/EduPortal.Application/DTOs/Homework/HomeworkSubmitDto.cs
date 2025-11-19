using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Homework;

public class HomeworkSubmitDto
{
    [Required]
    public int HomeworkId { get; set; }

    [Required]
    public int StudentId { get; set; }

    [StringLength(500)]
    public string? SubmissionUrl { get; set; }

    [StringLength(1000)]
    public string? Comment { get; set; }

    public HomeworkStatus Status { get; set; } = HomeworkStatus.TeslimEdildi;

    public DateTime? SubmissionDate { get; set; } = DateTime.UtcNow;
}
