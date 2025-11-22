namespace EduPortal.Application.DTOs.Email;

public class EmailAttachmentDto
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public byte[]? FileContent { get; set; }
}
