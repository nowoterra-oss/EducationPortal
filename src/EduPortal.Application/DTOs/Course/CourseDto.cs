using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Course;

public class CourseDto
{
    public int Id { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string? CourseCode { get; set; }
    public string? Subject { get; set; }
    public string? Level { get; set; }
    public int? Credits { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int CurriculumCount { get; set; }
    public int ResourceCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCourseDto
{
    [Required(ErrorMessage = "Ders adı belirtilmelidir")]
    [MaxLength(200, ErrorMessage = "Ders adı en fazla 200 karakter olabilir")]
    public string CourseName { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "Ders kodu en fazla 50 karakter olabilir")]
    public string? CourseCode { get; set; }

    [Required(ErrorMessage = "Konu alanı belirtilmelidir")]
    [MaxLength(100, ErrorMessage = "Konu alanı en fazla 100 karakter olabilir")]
    public string? Subject { get; set; }

    [MaxLength(50, ErrorMessage = "Seviye en fazla 50 karakter olabilir")]
    public string? Level { get; set; }

    public int? Credits { get; set; }

    [MaxLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

public class UpdateCourseDto
{
    [Required(ErrorMessage = "Ders adı belirtilmelidir")]
    [MaxLength(200, ErrorMessage = "Ders adı en fazla 200 karakter olabilir")]
    public string CourseName { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "Ders kodu en fazla 50 karakter olabilir")]
    public string? CourseCode { get; set; }

    [Required(ErrorMessage = "Konu alanı belirtilmelidir")]
    [MaxLength(100, ErrorMessage = "Konu alanı en fazla 100 karakter olabilir")]
    public string? Subject { get; set; }

    [MaxLength(50, ErrorMessage = "Seviye en fazla 50 karakter olabilir")]
    public string? Level { get; set; }

    public int? Credits { get; set; }

    [MaxLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
    public string? Description { get; set; }

    public bool IsActive { get; set; }
}

