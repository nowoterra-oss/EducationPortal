using System.ComponentModel.DataAnnotations;
using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.Announcement;

public class AnnouncementDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public AnnouncementType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string PublishedBy { get; set; } = string.Empty;
    public string PublisherName { get; set; } = string.Empty;
    public DateTime? PublishedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsPinned { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? TargetAudience { get; set; }
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAnnouncementDto
{
    [Required(ErrorMessage = "Duyuru başlığı belirtilmelidir")]
    [MaxLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Duyuru içeriği belirtilmelidir")]
    [MaxLength(5000, ErrorMessage = "İçerik en fazla 5000 karakter olabilir")]
    public string Content { get; set; } = string.Empty;

    public AnnouncementType Type { get; set; } = AnnouncementType.General;

    public DateTime? ExpiryDate { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsPinned { get; set; } = false;

    [MaxLength(500, ErrorMessage = "Ek URL en fazla 500 karakter olabilir")]
    public string? AttachmentUrl { get; set; }

    [MaxLength(200, ErrorMessage = "Hedef kitle en fazla 200 karakter olabilir")]
    public string? TargetAudience { get; set; }
}

public class UpdateAnnouncementDto
{
    [Required(ErrorMessage = "Duyuru başlığı belirtilmelidir")]
    [MaxLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Duyuru içeriği belirtilmelidir")]
    [MaxLength(5000, ErrorMessage = "İçerik en fazla 5000 karakter olabilir")]
    public string Content { get; set; } = string.Empty;

    public AnnouncementType Type { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public bool IsActive { get; set; }

    public bool IsPinned { get; set; }

    [MaxLength(500, ErrorMessage = "Ek URL en fazla 500 karakter olabilir")]
    public string? AttachmentUrl { get; set; }

    [MaxLength(200, ErrorMessage = "Hedef kitle en fazla 200 karakter olabilir")]
    public string? TargetAudience { get; set; }
}
