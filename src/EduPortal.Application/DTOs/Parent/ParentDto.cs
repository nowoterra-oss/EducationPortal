namespace EduPortal.Application.DTOs.Parent;

public class ParentDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }

    public string? Occupation { get; set; }
    public string? WorkPhone { get; set; }

    // Kimlik Bilgileri
    public string? IdentityType { get; set; }
    public string? IdentityNumber { get; set; }
    public string? Nationality { get; set; }

    // Kişisel Bilgiler
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }

    // Adres Bilgileri
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Address { get; set; }

    // İlişkili öğrenciler
    public List<ParentStudentInfo> Students { get; set; } = new List<ParentStudentInfo>();

    public DateTime CreatedDate { get; set; }
}

public class ParentStudentInfo
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? StudentNo { get; set; }
    public string Relationship { get; set; } = string.Empty;
    public bool IsPrimaryContact { get; set; }
    public bool IsEmergencyContact { get; set; }
}
