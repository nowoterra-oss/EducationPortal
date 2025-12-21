using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Student;

public class ActivityDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Organization { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsOngoing { get; set; } = false;
    public string? Achievements { get; set; }
}

public class ActivityCreateDto
{
    [Required(ErrorMessage = "Aktivite adi zorunludur")]
    [MaxLength(200, ErrorMessage = "Aktivite adi en fazla 200 karakter olabilir")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "Tur en fazla 100 karakter olabilir")]
    public string? Type { get; set; }

    [MaxLength(200, ErrorMessage = "Organizasyon en fazla 200 karakter olabilir")]
    public string? Organization { get; set; }

    [MaxLength(1000, ErrorMessage = "Aciklama en fazla 1000 karakter olabilir")]
    public string? Description { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsOngoing { get; set; } = false;

    [MaxLength(1000, ErrorMessage = "Kazanimlar en fazla 1000 karakter olabilir")]
    public string? Achievements { get; set; }
}
