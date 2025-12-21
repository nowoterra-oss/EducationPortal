using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Student;

public class ActivityDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class ActivityCreateDto
{
    [Required(ErrorMessage = "Aktivite adi zorunludur")]
    [MaxLength(200, ErrorMessage = "Aktivite adi en fazla 200 karakter olabilir")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "Kategori en fazla 100 karakter olabilir")]
    public string? Category { get; set; }

    [MaxLength(1000, ErrorMessage = "Aciklama en fazla 1000 karakter olabilir")]
    public string? Description { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
