using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class Counselor : BaseEntity
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Specialization { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation Properties
    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; } = null!;

    public virtual ICollection<StudentCounselorAssignment> Students { get; set; } = new List<StudentCounselorAssignment>();
    public virtual ICollection<CounselingMeeting> CounselingMeetings { get; set; } = new List<CounselingMeeting>();
}
