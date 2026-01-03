namespace EduPortal.Domain.Enums;

/// <summary>
/// Toplu mesaj hedef kitlesi
/// </summary>
[Flags]
public enum BroadcastTargetAudience
{
    None = 0,
    Students = 1,
    Teachers = 2,
    Parents = 4,
    Counselors = 8,
    All = Students | Teachers | Parents | Counselors
}
