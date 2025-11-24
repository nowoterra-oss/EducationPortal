using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Course;

public class CurriculumDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string TopicName { get; set; } = string.Empty;
    public int TopicOrder { get; set; }
    public string? Description { get; set; }
    public int? EstimatedHours { get; set; }
    public bool IsCompleted { get; set; }
    public List<CourseResourceDto> Resources { get; set; } = new();
}

public class CreateCurriculumDto
{
    [Required(ErrorMessage = "Konu adı belirtilmelidir")]
    [MaxLength(200, ErrorMessage = "Konu adı en fazla 200 karakter olabilir")]
    public string TopicName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Konu sırası belirtilmelidir")]
    public int TopicOrder { get; set; }

    [MaxLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
    public string? Description { get; set; }

    public int? EstimatedHours { get; set; }

    public bool IsCompleted { get; set; } = false;
}

public class UpdateCurriculumDto
{
    public List<CurriculumItemDto> Items { get; set; } = new();
}

public class CurriculumItemDto
{
    public int? Id { get; set; } // null for new items

    [Required(ErrorMessage = "Konu adı belirtilmelidir")]
    [MaxLength(200, ErrorMessage = "Konu adı en fazla 200 karakter olabilir")]
    public string TopicName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Konu sırası belirtilmelidir")]
    public int TopicOrder { get; set; }

    [MaxLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
    public string? Description { get; set; }

    public int? EstimatedHours { get; set; }

    public bool IsCompleted { get; set; } = false;
}
