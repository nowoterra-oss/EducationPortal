namespace EduPortal.Application.DTOs.Parent;

public class ParentSummaryDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Occupation { get; set; }
    public int StudentCount { get; set; }
}
