namespace EduPortal.Application.DTOs.Parent;

public class ParentDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }

    public string? Occupation { get; set; }
    public string? WorkPhone { get; set; }

    // İlişkili öğrenciler
    public List<ParentStudentInfo> Students { get; set; } = new List<ParentStudentInfo>();

    public DateTime CreatedDate { get; set; }
}

public class ParentStudentInfo
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public bool IsPrimaryContact { get; set; }
    public bool IsEmergencyContact { get; set; }
}
