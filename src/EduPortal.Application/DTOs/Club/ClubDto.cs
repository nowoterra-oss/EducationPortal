using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Club;

public class ClubDto
{
    public int Id { get; set; }
    public string ClubName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? AdvisorTeacherId { get; set; }
    public string? AdvisorName { get; set; }
    public int MaxMembers { get; set; }
    public int CurrentMembers { get; set; }
    public string? MeetingDay { get; set; }
    public string? MeetingTime { get; set; }
    public string? MeetingRoom { get; set; }
    public bool IsActive { get; set; }
    public bool AcceptingMembers { get; set; }
    public string? AcademicYear { get; set; }
    public string? Requirements { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateClubDto
{
    [Required(ErrorMessage = "Kulüp adı belirtilmelidir")]
    [MaxLength(100, ErrorMessage = "Kulüp adı en fazla 100 karakter olabilir")]
    public string ClubName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kulüp açıklaması belirtilmelidir")]
    [MaxLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
    public string Description { get; set; } = string.Empty;

    public int? AdvisorTeacherId { get; set; }

    [Range(1, 100, ErrorMessage = "Maksimum üye sayısı 1-100 arasında olmalıdır")]
    public int MaxMembers { get; set; } = 30;

    [MaxLength(100, ErrorMessage = "Toplantı günü en fazla 100 karakter olabilir")]
    public string? MeetingDay { get; set; }

    [MaxLength(50, ErrorMessage = "Toplantı saati en fazla 50 karakter olabilir")]
    public string? MeetingTime { get; set; }

    [MaxLength(100, ErrorMessage = "Toplantı odası en fazla 100 karakter olabilir")]
    public string? MeetingRoom { get; set; }

    public bool IsActive { get; set; } = true;
    public bool AcceptingMembers { get; set; } = true;

    [MaxLength(20, ErrorMessage = "Akademik yıl en fazla 20 karakter olabilir")]
    public string? AcademicYear { get; set; }

    [MaxLength(500, ErrorMessage = "Gereksinimler en fazla 500 karakter olabilir")]
    public string? Requirements { get; set; }
}

public class UpdateClubDto
{
    [Required(ErrorMessage = "Kulüp adı belirtilmelidir")]
    [MaxLength(100, ErrorMessage = "Kulüp adı en fazla 100 karakter olabilir")]
    public string ClubName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kulüp açıklaması belirtilmelidir")]
    [MaxLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
    public string Description { get; set; } = string.Empty;

    public int? AdvisorTeacherId { get; set; }

    [Range(1, 100, ErrorMessage = "Maksimum üye sayısı 1-100 arasında olmalıdır")]
    public int MaxMembers { get; set; }

    [MaxLength(100, ErrorMessage = "Toplantı günü en fazla 100 karakter olabilir")]
    public string? MeetingDay { get; set; }

    [MaxLength(50, ErrorMessage = "Toplantı saati en fazla 50 karakter olabilir")]
    public string? MeetingTime { get; set; }

    [MaxLength(100, ErrorMessage = "Toplantı odası en fazla 100 karakter olabilir")]
    public string? MeetingRoom { get; set; }

    public bool IsActive { get; set; }
    public bool AcceptingMembers { get; set; }

    [MaxLength(20, ErrorMessage = "Akademik yıl en fazla 20 karakter olabilir")]
    public string? AcademicYear { get; set; }

    [MaxLength(500, ErrorMessage = "Gereksinimler en fazla 500 karakter olabilir")]
    public string? Requirements { get; set; }
}

public class ClubMemberDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? StudentNo { get; set; }
    public string ClubType { get; set; } = string.Empty;
    public string? Role { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
