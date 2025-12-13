using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class TeacherBranch
{
    [Key]
    public int Id { get; set; }

    public int TeacherId { get; set; }

    public int CourseId { get; set; }

    [ForeignKey(nameof(TeacherId))]
    public virtual Teacher? Teacher { get; set; }

    [ForeignKey(nameof(CourseId))]
    public virtual Course? Course { get; set; }
}
