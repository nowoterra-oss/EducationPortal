namespace EduPortal.Application.DTOs.Class;

public class ClassStatisticsDto
{
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;

    // Kapasite bilgileri
    public int Capacity { get; set; }
    public int CurrentStudentCount { get; set; }
    public decimal OccupancyRate { get; set; }

    // Öğrenci istatistikleri
    public int MaleStudentCount { get; set; }
    public int FemaleStudentCount { get; set; }

    // Akademik istatistikler
    public int TotalHomeworks { get; set; }
    public int CompletedHomeworks { get; set; }
    public decimal HomeworkCompletionRate { get; set; }

    public int TotalExams { get; set; }
    public decimal AverageExamScore { get; set; }

    // Devam istatistikleri
    public decimal AverageAttendanceRate { get; set; }
}
