namespace EduPortal.Application.DTOs.CounselorDashboard;

public class StudentActivitiesAndAwardsDto
{
    public int StudentId { get; set; }

    // Aktiviteler (Sanatsal, Sportif)
    public List<ActivityDto> Activities { get; set; } = new();

    // Yaz Aktiviteleri
    public List<SummerActivityDto> SummerActivities { get; set; } = new();

    // Stajlar
    public List<InternshipDto> Internships { get; set; } = new();

    // Yarisma Sonuclari
    public List<CompetitionDto> Competitions { get; set; } = new();

    // Sosyal Sorumluluk Projeleri
    public List<SocialProjectDto> SocialProjects { get; set; } = new();

    // Kulup Uyelikleri
    public List<ClubMembershipDto> ClubMemberships { get; set; } = new();

    // Oduller (Scope'a gore gruplu)
    public AwardsSummaryDto Awards { get; set; } = new();

    // Characterix Sonuclari
    public CharacterixSummaryDto? Characterix { get; set; }
}

public class ActivityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Artistic", "Athletic"
    public string? Level { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class SummerActivityDto
{
    public int Id { get; set; }
    public string ActivityName { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public string? Organization { get; set; }
    public string Year { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Description { get; set; }
}

public class InternshipDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? Position { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Description { get; set; }
}

public class CompetitionDto
{
    public int Id { get; set; }
    public string CompetitionName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Result { get; set; }
    public DateTime? Date { get; set; }
    public string? Description { get; set; }
}

public class SocialProjectDto
{
    public int Id { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string? Role { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Description { get; set; }
    public string? Impact { get; set; }
}

public class ClubMembershipDto
{
    public int Id { get; set; }
    public string ClubName { get; set; } = string.Empty;
    public string? Role { get; set; }
    public DateTime? JoinDate { get; set; }
    public bool IsActive { get; set; }
}

public class AwardsSummaryDto
{
    public List<AwardDto> International { get; set; } = new();
    public List<AwardDto> National { get; set; } = new();
    public List<AwardDto> Local { get; set; } = new();
    public int TotalCount { get; set; }
}

public class AwardDto
{
    public int Id { get; set; }
    public string AwardName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Rank { get; set; }
    public string? IssuingOrganization { get; set; }
    public DateTime? AwardDate { get; set; }
}

public class CharacterixSummaryDto
{
    public int Id { get; set; }
    public DateTime AssessmentDate { get; set; }
    public string? Analysis { get; set; }
    public string? Interpretation { get; set; }
    public string? Recommendations { get; set; }
    public string? ReportUrl { get; set; }
}
