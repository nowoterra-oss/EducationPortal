namespace EduPortal.Domain.Enums;

public enum AvailabilityType
{
    Available = 0,      // Boş - müsait
    Busy = 1,          // Dolu - ders var
    Unavailable = 2,   // Müsait değil - bloklanmış
    School = 3,        // Okul saati
    Break = 4          // Ara/Teneffüs
}
