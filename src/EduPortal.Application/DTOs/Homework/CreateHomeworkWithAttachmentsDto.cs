using Microsoft.AspNetCore.Http;

namespace EduPortal.Application.DTOs.Homework;

public class CreateHomeworkWithAttachmentsDto
{
    public int StudentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public int? CurriculumId { get; set; }
    public int? CourseId { get; set; }

    // Dosya yükleme seçenekleri
    public List<int>? CourseResourceIds { get; set; } // Mevcut ders kaynaklarından
    public List<IFormFile>? UploadedFiles { get; set; } // Yeni dosyalar
}
