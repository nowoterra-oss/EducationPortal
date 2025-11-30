namespace EduPortal.Application.DTOs.File;

public class FileUploadResultDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

public class ProfilePhotoUploadResultDto
{
    public string PhotoUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
