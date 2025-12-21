using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Student;

public class ForeignLanguageDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string Language { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
}

public class ForeignLanguageCreateDto
{
    [Required(ErrorMessage = "Dil adi zorunludur")]
    [MaxLength(100, ErrorMessage = "Dil adi en fazla 100 karakter olabilir")]
    public string Language { get; set; } = string.Empty;

    [Required(ErrorMessage = "Dil seviyesi zorunludur")]
    [MaxLength(10, ErrorMessage = "Dil seviyesi en fazla 10 karakter olabilir")]
    public string Level { get; set; } = string.Empty;
}
