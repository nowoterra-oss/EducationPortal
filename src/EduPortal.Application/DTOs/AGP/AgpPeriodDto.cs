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

    public string Title { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
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

    public string Title { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

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
    public bool IsMilestone { get; set; } = false;
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
    public bool IsMilestone { get; set; }
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
