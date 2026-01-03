namespace EduPortal.Application.DTOs.Parent;

/// <summary>
/// Parent DTO with student information for list views
/// </summary>
public class ParentWithStudentDto : ParentDto
{
    /// <summary>
    /// İlişkili öğrencinin adı
    /// </summary>
    public string? StudentFirstName { get; set; }

    /// <summary>
    /// İlişkili öğrencinin soyadı
    /// </summary>
    public string? StudentLastName { get; set; }

    /// <summary>
    /// İlişkili öğrencinin numarası
    /// </summary>
    public string? StudentNo { get; set; }

    /// <summary>
    /// İlişkili öğrencinin ID'si
    /// </summary>
    public int? StudentId { get; set; }

    /// <summary>
    /// Veli-öğrenci ilişki türü (Anne, Baba, Vasi)
    /// </summary>
    public string? Relationship { get; set; }
}
