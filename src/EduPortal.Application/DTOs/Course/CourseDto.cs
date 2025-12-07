using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EduPortal.Application.DTOs.Course;

public class CourseDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("courseName")]
    public string CourseName { get; set; } = string.Empty;

    [JsonPropertyName("courseCode")]
    public string CourseCode { get; set; } = string.Empty;
}

public class CreateCourseDto
{
    [Required(ErrorMessage = "Ders adı belirtilmelidir")]
    [MaxLength(200, ErrorMessage = "Ders adı en fazla 200 karakter olabilir")]
    [JsonPropertyName("courseName")]
    public string CourseName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ders kodu belirtilmelidir")]
    [MaxLength(20, ErrorMessage = "Ders kodu en fazla 20 karakter olabilir")]
    [JsonPropertyName("courseCode")]
    public string CourseCode { get; set; } = string.Empty;
}

public class UpdateCourseDto
{
    [Required(ErrorMessage = "Ders adı belirtilmelidir")]
    [MaxLength(200, ErrorMessage = "Ders adı en fazla 200 karakter olabilir")]
    [JsonPropertyName("courseName")]
    public string CourseName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ders kodu belirtilmelidir")]
    [MaxLength(20, ErrorMessage = "Ders kodu en fazla 20 karakter olabilir")]
    [JsonPropertyName("courseCode")]
    public string CourseCode { get; set; } = string.Empty;
}
