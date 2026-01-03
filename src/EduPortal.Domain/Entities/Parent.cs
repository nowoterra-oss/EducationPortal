using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduPortal.Domain.Entities;

public class Parent : BaseEntity
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Occupation { get; set; }

    [MaxLength(20)]
    public string? WorkPhone { get; set; }

    /// <summary>
    /// Kimlik belgesi türü (TC Kimlik, Pasaport, Yabancı Kimlik, vb.)
    /// </summary>
    public IdentityType IdentityType { get; set; } = IdentityType.TCKimlik;

    /// <summary>
    /// Kimlik numarası (TC Kimlik No, Pasaport No, vb.)
    /// </summary>
    [MaxLength(50)]
    public string? IdentityNumber { get; set; }

    /// <summary>
    /// Uyruk/Vatandaşlık
    /// </summary>
    [MaxLength(100)]
    public string? Nationality { get; set; } = "TR";

    /// <summary>
    /// Doğum tarihi
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Cinsiyet
    /// </summary>
    public Gender? Gender { get; set; }

    /// <summary>
    /// İl
    /// </summary>
    [MaxLength(100)]
    public string? City { get; set; }

    /// <summary>
    /// İlçe
    /// </summary>
    [MaxLength(100)]
    public string? District { get; set; }

    /// <summary>
    /// Adres detayı
    /// </summary>
    [MaxLength(500)]
    public string? Address { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; } = null!;

    // N:N ilişki için
    public virtual ICollection<StudentParent> Students { get; set; } = new List<StudentParent>();
}
