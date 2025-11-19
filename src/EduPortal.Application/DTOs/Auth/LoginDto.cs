using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Auth;

public class LoginDto
{
    [Required(ErrorMessage = "Email alanı zorunludur")]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre alanı zorunludur")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
