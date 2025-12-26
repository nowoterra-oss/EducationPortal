namespace EduPortal.Application.DTOs.Branch;

public class BranchStatisticsDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;

    // Student Statistics
    public int TotalStudents { get; set; }
    public int ActiveStudents { get; set; }
    public int NewStudentsThisMonth { get; set; }

    // Staff Statistics
    public int TotalTeachers { get; set; }
    public int TotalCounselors { get; set; }

    // Academic Statistics
    public int TotalClasses { get; set; }
    public int TotalClassrooms { get; set; }

    // Counseling Statistics
    public int ActiveCounselingPrograms { get; set; }
    public int CompletedMeetingsThisMonth { get; set; }

    // Financial Statistics (optional)
    public decimal MonthlyRevenue { get; set; }
    public decimal CounselingRevenue { get; set; }

    // Capacity
    public int Capacity { get; set; }
    public decimal CapacityUtilization { get; set; }
}
