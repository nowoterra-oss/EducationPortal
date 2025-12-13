using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EduPortal.Domain.Enums;

namespace EduPortal.Domain.Entities;

public class TeacherWorkType
{
    [Key]
    public int Id { get; set; }

    public int TeacherId { get; set; }

    public WorkType WorkType { get; set; }

    [ForeignKey(nameof(TeacherId))]
    public virtual Teacher? Teacher { get; set; }
}
