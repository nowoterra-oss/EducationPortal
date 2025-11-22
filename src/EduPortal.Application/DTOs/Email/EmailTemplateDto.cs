namespace EduPortal.Application.DTOs.Email;

public class EmailTemplateDto
{
    public int Id { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string TemplateType { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public Dictionary<string, string> Variables { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
