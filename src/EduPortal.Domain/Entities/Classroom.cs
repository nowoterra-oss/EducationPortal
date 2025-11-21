using EduPortal.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace EduPortal.Domain.Entities;

public class Classroom : BaseAuditableEntity
{
    [Required]
    [MaxLength(50)]
    public string RoomNumber { get; set; } = string.Empty; // "A-101", "B-205"

    [Required]
    [MaxLength(100)]
    public string BuildingName { get; set; } = string.Empty; // "Ana Bina", "Fen Blok"

    [Required]
    public int Capacity { get; set; }

    [MaxLength(50)]
    public string? Floor { get; set; } // "Zemin Kat", "1. Kat"

    [MaxLength(500)]
    public string? Equipment { get; set; } // "Projeksiyon, Akıllı Tahta, Bilgisayar"

    public bool HasProjector { get; set; } = false;

    public bool HasSmartBoard { get; set; } = false;

    public bool HasComputer { get; set; } = false;

    public bool IsAvailable { get; set; } = true;

    public bool IsLab { get; set; } = false;

    [MaxLength(500)]
    public string? Notes { get; set; }
}
