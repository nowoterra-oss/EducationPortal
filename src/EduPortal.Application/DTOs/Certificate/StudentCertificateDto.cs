using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Certificate;

public class StudentCertificateDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadDate { get; set; }
    public DateTime? IssueDate { get; set; }
    public string? IssuingOrganization { get; set; }
}

public class StudentCertificateCreateDto
{
    [Required(ErrorMessage = "Sertifika adı zorunludur")]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime? IssueDate { get; set; }

    [MaxLength(255)]
    public string? IssuingOrganization { get; set; }
}

public class StudentCertificateUploadResultDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}
