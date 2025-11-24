using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Parent;

public class UpdateParentDto
{
    [MaxLength(200)]
    public string? Occupation { get; set; }

    [MaxLength(20)]
    public string? WorkPhone { get; set; }
}
