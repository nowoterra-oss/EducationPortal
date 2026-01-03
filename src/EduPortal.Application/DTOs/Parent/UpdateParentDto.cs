using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Parent;

public class UpdateParentDto
{
    [MaxLength(200)]
    public string? Occupation { get; set; }

    [MaxLength(20)]
    public string? WorkPhone { get; set; }

    /// <summary>
    /// Ad (User tablosunda güncellenir)
    /// </summary>
    [MaxLength(100)]
    public string? FirstName { get; set; }

    /// <summary>
    /// Soyad (User tablosunda güncellenir)
    /// </summary>
    [MaxLength(100)]
    public string? LastName { get; set; }

    /// <summary>
    /// Email (User tablosunda güncellenir)
    /// </summary>
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
    [MaxLength(256)]
    public string? Email { get; set; }

    /// <summary>
    /// Telefon numarası (User tablosunda güncellenir)
    /// </summary>
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Kimlik türü
    /// </summary>
    public int? IdentityType { get; set; }

    /// <summary>
    /// Kimlik numarası
    /// </summary>
    [MaxLength(50)]
    public string? IdentityNumber { get; set; }

    /// <summary>
    /// Uyruk
    /// </summary>
    [MaxLength(100)]
    public string? Nationality { get; set; }

    /// <summary>
    /// Doğum tarihi
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Cinsiyet
    /// </summary>
    public int? Gender { get; set; }

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
}
