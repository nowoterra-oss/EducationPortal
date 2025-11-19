using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Auth;

public class ChangePasswordDto
{
    [Required(ErrorMessage = "Mevcut şifre zorunludur")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yeni şifre zorunludur")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Şifre en az 8 karakter olmalıdır")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yeni şifre tekrarı zorunludur")]
    [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor")]
    [DataType(DataType.Password)]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
