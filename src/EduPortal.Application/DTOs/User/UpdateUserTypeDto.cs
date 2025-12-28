using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.User;

public class UpdateUserTypeDto
{
    [Required(ErrorMessage = "Kullanıcı tipi zorunludur")]
    public string UserType { get; set; } = string.Empty;
}
