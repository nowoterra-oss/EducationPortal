using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.Finance;

public class TeacherSalaryCreateDto
{
    public int TeacherId { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal Bonus { get; set; } = 0;
    public decimal Deduction { get; set; } = 0;
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTime DueDate { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
}

public class TeacherSalaryPayDto
{
    public PaymentMethod PaymentMethod { get; set; }
    public string? TransactionId { get; set; }
    public string? Notes { get; set; }
}

public class TeacherSalaryBulkCreateDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTime DueDate { get; set; }
    public string? Description { get; set; }

    /// <summary>
    /// Eski yöntem: Sadece öğretmen ID'leri ile maaş oluşturma (varsayılan maaş kullanılır)
    /// </summary>
    public List<int>? TeacherIds { get; set; }

    /// <summary>
    /// Eski yöntem için varsayılan maaş
    /// </summary>
    public decimal? DefaultBaseSalary { get; set; }

    /// <summary>
    /// Yeni yöntem: Her öğretmen için özel maaş bilgisi
    /// </summary>
    public List<TeacherSalaryItemDto>? TeacherSalaries { get; set; }
}

/// <summary>
/// Toplu maaş oluşturmada her öğretmen için özel maaş bilgisi
/// </summary>
public class TeacherSalaryItemDto
{
    public int TeacherId { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal? Bonus { get; set; }
    public decimal? Deductions { get; set; }

    /// <summary>
    /// Saatlik çalışanlar için çalışılan saat
    /// </summary>
    public int? WorkedHours { get; set; }

    /// <summary>
    /// Saatlik ücret (öğretmenin tanımlı ücreti yerine kullanılabilir)
    /// </summary>
    public decimal? HourlyRate { get; set; }

    /// <summary>
    /// Maaş tipi: 0 = Monthly (Aylık), 1 = Hourly (Saatlik)
    /// </summary>
    public int? SalaryType { get; set; }

    /// <summary>
    /// Başlangıç yılı (belirtilmezse request.Year kullanılır)
    /// </summary>
    public int? Year { get; set; }

    /// <summary>
    /// Başlangıç ayı (belirtilmezse request.Month kullanılır)
    /// </summary>
    public int? Month { get; set; }

    /// <summary>
    /// Aylık maaş için süre (kaç ay). Varsayılan 1.
    /// Örneğin 10 verilirse başlangıç ayından itibaren 10 ayrı maaş kaydı oluşturulur.
    /// </summary>
    public int? DurationMonths { get; set; }

    public string? Notes { get; set; }
}
