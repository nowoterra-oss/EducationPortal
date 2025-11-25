using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Competition;

public class CompetitionDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? StudentNo { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Level { get; set; }
    public string? Achievement { get; set; }
    public DateTime? Date { get; set; }
    public string? DocumentUrl { get; set; }
    public string? Description { get; set; }
}

public class CreateCompetitionDto
{
    [Required(ErrorMessage = "Öğrenci belirtilmelidir")]
    public int StudentId { get; set; }

    [Required(ErrorMessage = "Yarışma/Ödül adı belirtilmelidir")]
    [MaxLength(200, ErrorMessage = "Ad en fazla 200 karakter olabilir")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "Kategori en fazla 50 karakter olabilir")]
    public string? Category { get; set; }

    [MaxLength(100, ErrorMessage = "Seviye en fazla 100 karakter olabilir")]
    public string? Level { get; set; }

    [MaxLength(100, ErrorMessage = "Başarı en fazla 100 karakter olabilir")]
    public string? Achievement { get; set; }

    public DateTime? Date { get; set; }

    [MaxLength(500, ErrorMessage = "Belge URL en fazla 500 karakter olabilir")]
    public string? DocumentUrl { get; set; }

    [MaxLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
    public string? Description { get; set; }
}

public class UpdateCompetitionDto
{
    [Required(ErrorMessage = "Yarışma/Ödül adı belirtilmelidir")]
    [MaxLength(200, ErrorMessage = "Ad en fazla 200 karakter olabilir")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "Kategori en fazla 50 karakter olabilir")]
    public string? Category { get; set; }

    [MaxLength(100, ErrorMessage = "Seviye en fazla 100 karakter olabilir")]
    public string? Level { get; set; }

    [MaxLength(100, ErrorMessage = "Başarı en fazla 100 karakter olabilir")]
    public string? Achievement { get; set; }

    public DateTime? Date { get; set; }

    [MaxLength(500, ErrorMessage = "Belge URL en fazla 500 karakter olabilir")]
    public string? DocumentUrl { get; set; }

    [MaxLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
    public string? Description { get; set; }
}
