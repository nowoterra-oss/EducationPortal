using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Email;

public class SendEmailDto
{
    [Required]
    [EmailAddress]
    public string To { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? ToName { get; set; }

    [Required]
    [MaxLength(300)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    public bool IsHtml { get; set; } = true;

    public List<EmailAttachmentDto>? Attachments { get; set; }

    public string? Cc { get; set; }

    public string? Bcc { get; set; }
}
