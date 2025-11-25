using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Classroom;

public class ClassroomDto
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string BuildingName { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string? Floor { get; set; }
    public string? Equipment { get; set; }
    public bool HasProjector { get; set; }
    public bool HasSmartBoard { get; set; }
    public bool HasComputer { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsLab { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateClassroomDto
{
    [Required(ErrorMessage = "Oda numarası belirtilmelidir")]
    [MaxLength(50, ErrorMessage = "Oda numarası en fazla 50 karakter olabilir")]
    public string RoomNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bina adı belirtilmelidir")]
    [MaxLength(100, ErrorMessage = "Bina adı en fazla 100 karakter olabilir")]
    public string BuildingName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kapasite belirtilmelidir")]
    [Range(1, 500, ErrorMessage = "Kapasite 1-500 arasında olmalıdır")]
    public int Capacity { get; set; }

    [MaxLength(50, ErrorMessage = "Kat bilgisi en fazla 50 karakter olabilir")]
    public string? Floor { get; set; }

    [MaxLength(500, ErrorMessage = "Ekipman bilgisi en fazla 500 karakter olabilir")]
    public string? Equipment { get; set; }

    public bool HasProjector { get; set; } = false;
    public bool HasSmartBoard { get; set; } = false;
    public bool HasComputer { get; set; } = false;
    public bool IsAvailable { get; set; } = true;
    public bool IsLab { get; set; } = false;

    public int? BranchId { get; set; }

    [MaxLength(500, ErrorMessage = "Notlar en fazla 500 karakter olabilir")]
    public string? Notes { get; set; }
}

public class UpdateClassroomDto
{
    [Required(ErrorMessage = "Oda numarası belirtilmelidir")]
    [MaxLength(50, ErrorMessage = "Oda numarası en fazla 50 karakter olabilir")]
    public string RoomNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bina adı belirtilmelidir")]
    [MaxLength(100, ErrorMessage = "Bina adı en fazla 100 karakter olabilir")]
    public string BuildingName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kapasite belirtilmelidir")]
    [Range(1, 500, ErrorMessage = "Kapasite 1-500 arasında olmalıdır")]
    public int Capacity { get; set; }

    [MaxLength(50, ErrorMessage = "Kat bilgisi en fazla 50 karakter olabilir")]
    public string? Floor { get; set; }

    [MaxLength(500, ErrorMessage = "Ekipman bilgisi en fazla 500 karakter olabilir")]
    public string? Equipment { get; set; }

    public bool HasProjector { get; set; }
    public bool HasSmartBoard { get; set; }
    public bool HasComputer { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsLab { get; set; }

    public int? BranchId { get; set; }

    [MaxLength(500, ErrorMessage = "Notlar en fazla 500 karakter olabilir")]
    public string? Notes { get; set; }
}

public class ClassroomScheduleDto
{
    public int ClassroomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? CourseName { get; set; }
    public string? TeacherName { get; set; }
}
