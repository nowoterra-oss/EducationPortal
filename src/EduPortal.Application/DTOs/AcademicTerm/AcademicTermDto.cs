using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.AcademicTerm;

public class AcademicTermDto
{
    public int Id { get; set; }
    public string TermName { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public int TermNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? MidtermStartDate { get; set; }
    public DateTime? MidtermEndDate { get; set; }
    public DateTime? FinalStartDate { get; set; }
    public DateTime? FinalEndDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsCurrent { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAcademicTermDto
{
    [Required(ErrorMessage = "Dönem adı belirtilmelidir")]
    [MaxLength(100, ErrorMessage = "Dönem adı en fazla 100 karakter olabilir")]
    public string TermName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Akademik yıl belirtilmelidir")]
    [MaxLength(20, ErrorMessage = "Akademik yıl en fazla 20 karakter olabilir")]
    public string AcademicYear { get; set; } = string.Empty;

    [Required(ErrorMessage = "Dönem numarası belirtilmelidir")]
    [Range(1, 2, ErrorMessage = "Dönem numarası 1 veya 2 olmalıdır")]
    public int TermNumber { get; set; }

    [Required(ErrorMessage = "Başlangıç tarihi belirtilmelidir")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Bitiş tarihi belirtilmelidir")]
    public DateTime EndDate { get; set; }

    public DateTime? MidtermStartDate { get; set; }
    public DateTime? MidtermEndDate { get; set; }
    public DateTime? FinalStartDate { get; set; }
    public DateTime? FinalEndDate { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsCurrent { get; set; } = false;

    [MaxLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
    public string? Description { get; set; }
}

public class UpdateAcademicTermDto
{
    [Required(ErrorMessage = "Dönem adı belirtilmelidir")]
    [MaxLength(100, ErrorMessage = "Dönem adı en fazla 100 karakter olabilir")]
    public string TermName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Akademik yıl belirtilmelidir")]
    [MaxLength(20, ErrorMessage = "Akademik yıl en fazla 20 karakter olabilir")]
    public string AcademicYear { get; set; } = string.Empty;

    [Required(ErrorMessage = "Dönem numarası belirtilmelidir")]
    [Range(1, 2, ErrorMessage = "Dönem numarası 1 veya 2 olmalıdır")]
    public int TermNumber { get; set; }

    [Required(ErrorMessage = "Başlangıç tarihi belirtilmelidir")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Bitiş tarihi belirtilmelidir")]
    public DateTime EndDate { get; set; }

    public DateTime? MidtermStartDate { get; set; }
    public DateTime? MidtermEndDate { get; set; }
    public DateTime? FinalStartDate { get; set; }
    public DateTime? FinalEndDate { get; set; }

    public bool IsActive { get; set; }

    [MaxLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
    public string? Description { get; set; }
}
