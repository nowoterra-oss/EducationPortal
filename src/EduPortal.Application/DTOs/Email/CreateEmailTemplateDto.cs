using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Email;

public class CreateEmailTemplateDto
{
    [Required]
    [MaxLength(200)]
    public string TemplateName { get; set; } = string.Empty;

    [Required]
    public int TemplateType { get; set; }

    [Required]
    [MaxLength(300)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? VariablesJson { get; set; }

    public bool IsActive { get; set; } = true;
}
