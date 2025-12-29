using System.Text.Json.Serialization;
using EduPortal.Domain.Enums;

namespace EduPortal.Application.DTOs.AGP;

/// <summary>
/// AGP Timeline dönem DTO'su
/// </summary>
public class AgpPeriodDto
{
    public int? Id { get; set; }

    /// <summary>
    /// Dönem adı (opsiyonel)
    /// </summary>
    public string? PeriodName { get; set; }

    public string? Title { get; set; }

    /// <summary>
    /// Dönem başlangıç tarihi (opsiyonel - boş ise AGP başlangıç tarihi kullanılır)
    /// </summary>
    public string? StartDate { get; set; }

    /// <summary>
    /// Dönem bitiş tarihi (opsiyonel - boş ise AGP bitiş tarihi kullanılır)
    /// </summary>
    public string? EndDate { get; set; }

    public string? Color { get; set; }
    public int Order { get; set; }
    public List<AgpMilestoneDto> Milestones { get; set; } = new();
    public List<AgpActivityDto> Activities { get; set; } = new();
}

/// <summary>
/// AGP Timeline dönem response DTO'su (Timeline için)
/// </summary>
public class AgpPeriodResponseDto
{
    public int Id { get; set; }
    public string? PeriodName { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Color { get; set; }
    public int Order { get; set; }
    public int AgpId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public List<AgpMilestoneResponseDto> Milestones { get; set; } = new();
    public List<AgpActivityResponseDto> Activities { get; set; } = new();

    // Timeline hesaplamaları
    public int TotalDays { get; set; }
    public int ElapsedDays { get; set; }
    public double ProgressPercentage { get; set; }
}

/// <summary>
/// AGP Timeline dönem oluşturma DTO'su
/// </summary>
public class AgpPeriodCreateDto
{
    /// <summary>
    /// Dönem adı (opsiyonel, boş bırakılırsa tarihlerden otomatik oluşturulur)
    /// </summary>
    public string? PeriodName { get; set; }

    public string? Title { get; set; }

    /// <summary>
    /// Dönem başlangıç tarihi (opsiyonel - boş ise AGP başlangıç tarihi kullanılır)
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Dönem bitiş tarihi (opsiyonel - boş ise AGP bitiş tarihi kullanılır)
    /// </summary>
    public DateTime? EndDate { get; set; }

    public string? Color { get; set; }

    public int Order { get; set; }

    public int AgpId { get; set; }

    public List<AgpMilestoneCreateDto> Milestones { get; set; } = new();

    public List<AgpActivityCreateDto> Activities { get; set; } = new();
}

/// <summary>
/// AGP Timeline dönem güncelleme DTO'su
/// </summary>
public class AgpPeriodUpdateDto
{
    public int Id { get; set; }
    public string? PeriodName { get; set; }
    public string? Title { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Color { get; set; }
    public int? Order { get; set; }
    public List<AgpMilestoneCreateDto>? Milestones { get; set; }
    public List<AgpActivityCreateDto>? Activities { get; set; }
}

/// <summary>
/// AGP Timeline sınav/hedef DTO'su (örn: "SAT Ağustos", "IELTS 29 Ağu.")
/// </summary>
public class AgpMilestoneDto
{
    public int? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string Type { get; set; } = "exam"; // "exam", "goal", "event"

    /// <summary>
    /// Frontend uyumluluğu için Category alias'ı (Type ile aynı değeri döndürür)
    /// </summary>
    public string Category
    {
        get => Type;
        set => Type = value;
    }

    public bool IsMilestone { get; set; } = false;

    /// <summary>
    /// Başvuru başlangıç tarihi (opsiyonel)
    /// </summary>
    [JsonPropertyName("applicationStartDate")]
    public string? ApplicationStartDate { get; set; }

    /// <summary>
    /// Başvuru bitiş tarihi (opsiyonel)
    /// </summary>
    [JsonPropertyName("applicationEndDate")]
    public string? ApplicationEndDate { get; set; }

    /// <summary>
    /// Sınav/hedef puanı (opsiyonel)
    /// </summary>
    [JsonPropertyName("score")]
    public decimal? Score { get; set; }

    /// <summary>
    /// Maksimum puan (opsiyonel)
    /// </summary>
    [JsonPropertyName("maxScore")]
    public decimal? MaxScore { get; set; }

    /// <summary>
    /// Sonuç notları (opsiyonel)
    /// </summary>
    [JsonPropertyName("resultNotes")]
    public string? ResultNotes { get; set; }

    /// <summary>
    /// Tamamlandı mı? (opsiyonel)
    /// </summary>
    [JsonPropertyName("isCompleted")]
    public bool? IsCompleted { get; set; }
}

/// <summary>
/// AGP Timeline sınav/hedef response DTO'su
/// </summary>
public class AgpMilestoneResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Color { get; set; }
    public string Type { get; set; } = "exam";

    /// <summary>
    /// Frontend uyumluluğu için Category alias'ı
    /// </summary>
    public string Category => Type;

    public bool IsMilestone { get; set; }

    /// <summary>
    /// Başvuru başlangıç tarihi (opsiyonel)
    /// </summary>
    [JsonPropertyName("applicationStartDate")]
    public DateTime? ApplicationStartDate { get; set; }

    /// <summary>
    /// Başvuru bitiş tarihi (opsiyonel)
    /// </summary>
    [JsonPropertyName("applicationEndDate")]
    public DateTime? ApplicationEndDate { get; set; }

    /// <summary>
    /// Sınav/hedef puanı (opsiyonel)
    /// </summary>
    [JsonPropertyName("score")]
    public decimal? Score { get; set; }

    /// <summary>
    /// Maksimum puan (opsiyonel)
    /// </summary>
    [JsonPropertyName("maxScore")]
    public decimal? MaxScore { get; set; }

    /// <summary>
    /// Sonuç notları (opsiyonel)
    /// </summary>
    [JsonPropertyName("resultNotes")]
    public string? ResultNotes { get; set; }

    /// <summary>
    /// Tamamlandı mı? (opsiyonel)
    /// </summary>
    [JsonPropertyName("isCompleted")]
    public bool? IsCompleted { get; set; }
}

/// <summary>
/// AGP Timeline sınav/hedef oluşturma DTO'su
/// </summary>
public class AgpMilestoneCreateDto
{
    public int? Id { get; set; } // Güncelleme için
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Color { get; set; }
    public string Type { get; set; } = "exam";
    public bool IsMilestone { get; set; } = false;

    /// <summary>
    /// Başvuru başlangıç tarihi (opsiyonel)
    /// </summary>
    [JsonPropertyName("applicationStartDate")]
    public DateTime? ApplicationStartDate { get; set; }

    /// <summary>
    /// Başvuru bitiş tarihi (opsiyonel)
    /// </summary>
    [JsonPropertyName("applicationEndDate")]
    public DateTime? ApplicationEndDate { get; set; }

    /// <summary>
    /// Sınav/hedef puanı (opsiyonel)
    /// </summary>
    [JsonPropertyName("score")]
    public decimal? Score { get; set; }

    /// <summary>
    /// Maksimum puan (opsiyonel)
    /// </summary>
    [JsonPropertyName("maxScore")]
    public decimal? MaxScore { get; set; }

    /// <summary>
    /// Sonuç notları (opsiyonel)
    /// </summary>
    [JsonPropertyName("resultNotes")]
    public string? ResultNotes { get; set; }

    /// <summary>
    /// Tamamlandı mı? (opsiyonel)
    /// </summary>
    [JsonPropertyName("isCompleted")]
    public bool? IsCompleted { get; set; }
}

/// <summary>
/// AGP Timeline aktivite DTO'su (örn: "SAT Eng- IELTS", 6 saat/hf)
/// </summary>
public class AgpActivityDto
{
    public int? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? HoursPerWeek { get; set; }
    public string? Notes { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public int OwnerType { get; set; } = 1;
    public string? Status { get; set; }
    public bool NeedsReview { get; set; } = false;
}

/// <summary>
/// AGP Timeline aktivite response DTO'su
/// </summary>
public class AgpActivityResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? HoursPerWeek { get; set; }
    public int OwnerType { get; set; }
    public ActivityStatus Status { get; set; }
    public bool NeedsReview { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// AGP Timeline aktivite oluşturma DTO'su
/// </summary>
public class AgpActivityCreateDto
{
    public int? Id { get; set; } // Güncelleme için
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? HoursPerWeek { get; set; }
    public int OwnerType { get; set; } = 1;
    public ActivityStatus Status { get; set; } = ActivityStatus.Planned;
    public bool NeedsReview { get; set; } = false;
    public string? Notes { get; set; }
}

/// <summary>
/// Timeline View DTO (Gantt Chart için)
/// </summary>
public class AgpTimelineViewDto
{
    public DateTime TimelineStart { get; set; }
    public DateTime TimelineEnd { get; set; }
    public DateTime Today { get; set; }
    public List<string> Months { get; set; } = new();
    public List<AgpPeriodResponseDto> Periods { get; set; } = new();
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int AgpId { get; set; }
}
