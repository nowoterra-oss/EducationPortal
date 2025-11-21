namespace EduPortal.Domain.Enums;

public enum TransferReason
{
    Relocation = 0,     // Taşınma
    Closer = 1,         // Daha yakın şube
    Capacity = 2,       // Kapasite doldu
    Quality = 3,        // Kalite/hizmet talebi
    Other = 4
}
