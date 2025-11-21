namespace EduPortal.Domain.Enums;

public enum PackageType
{
    // Dershane
    TuitionStandard = 0,
    TuitionPremium = 1,
    TuitionIntensive = 2,

    // Koçluk
    CareerCoaching = 10,
    LifeCoaching = 11,
    SchoolSelection = 12,
    SportsCoaching = 13,

    // Yurt dışı
    StudyAbroadBasic = 20,
    StudyAbroadPremium = 21,
    StudyAbroadComplete = 22,

    // Combo
    ComboAcademic = 30,
    ComboAbroad = 31,
    ComboComplete = 32
}
