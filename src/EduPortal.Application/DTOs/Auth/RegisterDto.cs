using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Auth;

public class RegisterDto
{
    [Required(ErrorMessage = "Ad alanı zorunludur")]
    [StringLength(100, ErrorMessage = "Ad en fazla 100 karakter olabilir")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad alanı zorunludur")]
    [StringLength(100, ErrorMessage = "Soyad en fazla 100 karakter olabilir")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email alanı zorunludur")]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Şifre alanı zorunludur")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Şifre en az 8 karakter olmalıdır")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre tekrarı zorunludur")]
    [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Rol seçimi zorunludur")]
    public string Role { get; set; } = string.Empty;
}
