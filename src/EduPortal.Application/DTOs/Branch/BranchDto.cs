namespace EduPortal.Application.DTOs.Branch;

public class BranchDto
{
    public int Id { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Main", "Branch", "Satellite"
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public int Capacity { get; set; }
    public DateTime OpeningDate { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }

    // Statistics
    public int CurrentStudentCount { get; set; }
    public int CurrentTeacherCount { get; set; }
    public int CurrentCounselorCount { get; set; }
    public decimal CapacityUtilization { get; set; } // Percentage

    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}
