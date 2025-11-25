using EduPortal.Application.DTOs.Auth;
using FluentValidation;

namespace EduPortal.Application.Validators.Auth;

public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordDtoValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Mevcut şifre zorunludur");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Yeni şifre zorunludur")
            .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır")
            .MaximumLength(100).WithMessage("Şifre en fazla 100 karakter olabilir")
            .Matches(@"[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir")
            .Matches(@"[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir")
            .Matches(@"[0-9]").WithMessage("Şifre en az bir rakam içermelidir")
            .Matches(@"[!@#$%^&*(),.?""':{}|<>]").WithMessage("Şifre en az bir özel karakter içermelidir")
            .NotEqual(x => x.CurrentPassword).WithMessage("Yeni şifre mevcut şifreden farklı olmalıdır");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("Şifre tekrarı zorunludur")
            .Equal(x => x.NewPassword).WithMessage("Şifreler eşleşmiyor");
    }
}
