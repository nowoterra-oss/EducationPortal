using System.ComponentModel.DataAnnotations;
using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.AGP;

public class AGPGoalDto
{
    public int Id { get; set; }
    public int AGPId { get; set; }
    public int Month { get; set; }
    public string MonthName => GetMonthName(Month);
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public MilestoneStatus Status { get; set; }
    public string StatusName => GetStatusName(Status);
    public int CompletionPercentage { get; set; }
    public string? Notes { get; set; }

    private static string GetMonthName(int month) => month switch
    {
        1 => "Ocak",
        2 => "Şubat",
        3 => "Mart",
        4 => "Nisan",
        5 => "Mayıs",
        6 => "Haziran",
        7 => "Temmuz",
        8 => "Ağustos",
        9 => "Eylül",
        10 => "Ekim",
        11 => "Kasım",
        12 => "Aralık",
        _ => month.ToString()
    };

    private static string GetStatusName(MilestoneStatus status) => status switch
    {
        MilestoneStatus.Bekliyor => "Bekliyor",
        MilestoneStatus.Devam => "Devam Ediyor",
        MilestoneStatus.Tamamlandi => "Tamamlandı",
        _ => status.ToString()
    };
}

public class CreateAGPGoalDto
{
    [Required(ErrorMessage = "Ay belirtilmelidir")]
    [Range(1, 12, ErrorMessage = "Ay 1-12 arasında olmalıdır")]
    public int Month { get; set; }

    [Required(ErrorMessage = "Başlık belirtilmelidir")]
    [MaxLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Başlangıç tarihi belirtilmelidir")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Bitiş tarihi belirtilmelidir")]
    public DateTime EndDate { get; set; }

    public MilestoneStatus Status { get; set; } = MilestoneStatus.Bekliyor;

    [Range(0, 100, ErrorMessage = "Tamamlanma yüzdesi 0-100 arasında olmalıdır")]
    public int CompletionPercentage { get; set; } = 0;

    [MaxLength(1000, ErrorMessage = "Notlar en fazla 1000 karakter olabilir")]
    public string? Notes { get; set; }
}

public class UpdateAGPGoalDto
{
    [Required(ErrorMessage = "Ay belirtilmelidir")]
    [Range(1, 12, ErrorMessage = "Ay 1-12 arasında olmalıdır")]
    public int Month { get; set; }

    [Required(ErrorMessage = "Başlık belirtilmelidir")]
    [MaxLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Başlangıç tarihi belirtilmelidir")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Bitiş tarihi belirtilmelidir")]
    public DateTime EndDate { get; set; }

    [Required(ErrorMessage = "Durum belirtilmelidir")]
    public MilestoneStatus Status { get; set; }

    [Range(0, 100, ErrorMessage = "Tamamlanma yüzdesi 0-100 arasında olmalıdır")]
    public int CompletionPercentage { get; set; }

    [MaxLength(1000, ErrorMessage = "Notlar en fazla 1000 karakter olabilir")]
    public string? Notes { get; set; }
}
