using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class Curriculum : BaseEntity
{
    [Required]
    public int CourseId { get; set; }

    [Required]
    [MaxLength(200)]
    public string TopicName { get; set; } = string.Empty;

    [Required]
    public int TopicOrder { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int? EstimatedHours { get; set; }

    public bool IsCompleted { get; set; } = false;

    // Sınav sistemi için
    public bool HasExam { get; set; } = false;
    public int? ExamResourceId { get; set; } // Sınav dosyası

    // Navigation Properties
    [ForeignKey(nameof(CourseId))]
    public virtual Course Course { get; set; } = null!;

    [ForeignKey(nameof(ExamResourceId))]
    public virtual CourseResource? ExamResource { get; set; }

    public virtual ICollection<CourseResource> Resources { get; set; } = new List<CourseResource>();
    public virtual ICollection<StudentCurriculumProgress> StudentProgresses { get; set; } = new List<StudentCurriculumProgress>();
}
