using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Email;

public class BulkEmailDto
{
    [Required]
    public List<string> Recipients { get; set; } = new();

    [Required]
    [MaxLength(300)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    public bool IsHtml { get; set; } = true;

    public DateTime? ScheduledDate { get; set; }
}
