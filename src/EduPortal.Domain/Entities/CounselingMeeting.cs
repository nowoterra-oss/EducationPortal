using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class CounselingMeeting : BaseAuditableEntity
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int CounselorId { get; set; }

    [Required]
    public DateTime MeetingDate { get; set; }

    public int? Duration { get; set; } // Minutes

    [Required]
    [MaxLength(5000)]
    public string Notes { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Assignments { get; set; }

    public DateTime? NextMeetingDate { get; set; }

    public bool SendEmailToParent { get; set; } = false;

    public bool SendSMSToParent { get; set; } = false;

    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    [ForeignKey(nameof(CounselorId))]
    public virtual Counselor Counselor { get; set; } = null!;
}
