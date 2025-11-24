using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Course;

public class CourseResourceDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public int? CurriculumId { get; set; }
    public string? CurriculumTopicName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string ResourceUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsVisible { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCourseResourceDto
{
    public int? CurriculumId { get; set; }

    [Required(ErrorMessage = "Başlık belirtilmelidir")]
    [MaxLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kaynak türü belirtilmelidir")]
    [MaxLength(50, ErrorMessage = "Kaynak türü en fazla 50 karakter olabilir")]
    public string ResourceType { get; set; } = string.Empty; // "PDF", "Video", "Link"

    [Required(ErrorMessage = "Kaynak URL belirtilmelidir")]
    [MaxLength(500, ErrorMessage = "Kaynak URL en fazla 500 karakter olabilir")]
    public string ResourceUrl { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
    public string? Description { get; set; }

    public bool IsVisible { get; set; } = true;
}
