namespace EduPortal.Application.DTOs.StudentGroup;

public class StudentGroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? GroupType { get; set; }
    public int? MaxCapacity { get; set; }
    public string? Color { get; set; }
    public bool IsActive { get; set; }
    public int MemberCount { get; set; }
    public List<StudentGroupMemberDto> Members { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class StudentGroupMemberDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentNo { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public int Grade { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool IsActive { get; set; }
}

public class CreateStudentGroupDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? GroupType { get; set; }
    public int? MaxCapacity { get; set; }
    public string? Color { get; set; }
    public List<int>? StudentIds { get; set; }
}

public class UpdateStudentGroupDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? GroupType { get; set; }
    public int? MaxCapacity { get; set; }
    public string? Color { get; set; }
    public bool? IsActive { get; set; }
}

public class AddStudentsToGroupDto
{
    public List<int> StudentIds { get; set; } = new();
}

public class RemoveStudentsFromGroupDto
{
    public List<int> StudentIds { get; set; } = new();
}
