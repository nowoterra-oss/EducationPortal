using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

/// <summary>
/// Danisma gorusme notlari - Her gorusmeden sonra yazilan notlar
/// </summary>
public class CounselorNote : BaseAuditableEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int CounselorId { get; set; }

    public int? CounselingMeetingId { get; set; }

    [Required]
    public DateTime NoteDate { get; set; }

    [MaxLength(200)]
    public string? Subject { get; set; }

    [Required]
    public string NoteContent { get; set; } = string.Empty;

    public string? AssignedTasks { get; set; }

    public DateTime? NextMeetingDate { get; set; }

    public bool SendEmailToParent { get; set; } = false;
    public bool SendSmsToParent { get; set; } = false;
    public bool EmailSent { get; set; } = false;
    public bool SmsSent { get; set; } = false;
    public DateTime? EmailSentAt { get; set; }
    public DateTime? SmsSentAt { get; set; }

    // Navigation
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    [ForeignKey(nameof(CounselorId))]
    public virtual Counselor Counselor { get; set; } = null!;

    [ForeignKey(nameof(CounselingMeetingId))]
    public virtual CounselingMeeting? CounselingMeeting { get; set; }
}
