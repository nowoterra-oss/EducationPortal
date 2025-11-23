using System.ComponentModel.DataAnnotations;

namespace EduPortal.Domain.Entities;

public class AuditLog
{
    [Key]
    public long Id { get; set; }

    [MaxLength(450)]
    public string? UserId { get; set; }

    [MaxLength(200)]
    public string? UserName { get; set; }

    [MaxLength(200)]
    public string? UserEmail { get; set; }

    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? EntityId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty;

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string? Changes { get; set; }

    [MaxLength(100)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public bool IsSuccessful { get; set; } = true;

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    [MaxLength(500)]
    public string? AdditionalInfo { get; set; }
}
