using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class Announcement : BaseAuditableEntity
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(5000)]
    public string Content { get; set; } = string.Empty;

    [Required]
    public AnnouncementType Type { get; set; } = AnnouncementType.General;

    [Required]
    public string PublishedBy { get; set; } = string.Empty;

    public DateTime? PublishedDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsPinned { get; set; } = false;

    [MaxLength(500)]
    public string? AttachmentUrl { get; set; }

    [MaxLength(200)]
    public string? TargetAudience { get; set; } // "Tüm Öğrenciler", "9. Sınıflar", "Öğretmenler"

    public int ViewCount { get; set; } = 0;

    [ForeignKey(nameof(PublishedBy))]
    public virtual ApplicationUser Publisher { get; set; } = null!;
}
