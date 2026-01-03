using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Parent;

/// <summary>
/// DTO for creating a parent directly for a student (creates both ApplicationUser and Parent)
/// Used by frontend ParentCreate form
/// </summary>
public class CreateParentForStudentDto
{
    /// <summary>
    /// Veli adı
    /// </summary>
    [Required(ErrorMessage = "Ad zorunludur")]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Veli soyadı
    /// </summary>
    [Required(ErrorMessage = "Soyad zorunludur")]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Email adresi
    /// </summary>
    [Required(ErrorMessage = "Email zorunludur")]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Telefon numarası (ülke kodu dahil)
    /// </summary>
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Kimlik türü ("TCKimlik", "Pasaport", "YabanciKimlik", "Diger")
    /// </summary>
    [MaxLength(50)]
    public string? IdentityType { get; set; } = "TCKimlik";

    /// <summary>
    /// Kimlik numarası
    /// </summary>
    [MaxLength(50)]
    public string? IdentityNumber { get; set; }

    /// <summary>
    /// Uyruk
    /// </summary>
    [MaxLength(100)]
    public string? Nationality { get; set; } = "TR";

    /// <summary>
    /// Veli tipi (Anne, Baba, Vasi)
    /// </summary>
    [Required(ErrorMessage = "Veli tipi zorunludur")]
    [MaxLength(50)]
    public string ParentType { get; set; } = string.Empty;

    /// <summary>
    /// Doğum tarihi
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Cinsiyet ("Erkek", "Kadin")
    /// </summary>
    [MaxLength(20)]
    public string? Gender { get; set; }

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

    /// <summary>
    /// Meslek
    /// </summary>
    [MaxLength(200)]
    public string? Occupation { get; set; }

    /// <summary>
    /// Birincil iletişim kişisi mi?
    /// </summary>
    public bool IsPrimaryContact { get; set; } = false;

    /// <summary>
    /// Acil durum iletişim kişisi mi?
    /// </summary>
    public bool IsEmergencyContact { get; set; } = false;
}
