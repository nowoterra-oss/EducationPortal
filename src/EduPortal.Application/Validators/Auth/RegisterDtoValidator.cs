using EduPortal.Application.DTOs.Auth;
using FluentValidation;

namespace EduPortal.Application.Validators.Auth;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    private static readonly string[] ValidRoles = { "Admin", "Teacher", "Student", "Parent", "Coach" };

    public RegisterDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad alanı zorunludur")
            .MaximumLength(100).WithMessage("Ad en fazla 100 karakter olabilir")
            .Matches(@"^[a-zA-ZçğıöşüÇĞİÖŞÜ\s]+$").WithMessage("Ad sadece harf içerebilir");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad alanı zorunludur")
            .MaximumLength(100).WithMessage("Soyad en fazla 100 karakter olabilir")
            .Matches(@"^[a-zA-ZçğıöşüÇĞİÖŞÜ\s]+$").WithMessage("Soyad sadece harf içerebilir");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email alanı zorunludur")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz")
            .MaximumLength(256).WithMessage("Email en fazla 256 karakter olabilir");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^(\+90|0)?[0-9]{10}$").WithMessage("Geçerli bir telefon numarası giriniz (örn: 5551234567)")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre alanı zorunludur")
            .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır")
            .MaximumLength(100).WithMessage("Şifre en fazla 100 karakter olabilir")
            .Matches(@"[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir")
            .Matches(@"[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir")
            .Matches(@"[0-9]").WithMessage("Şifre en az bir rakam içermelidir")
            .Matches(@"[!@#$%^&*(),.?""':{}|<>]").WithMessage("Şifre en az bir özel karakter içermelidir");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Şifre tekrarı zorunludur")
            .Equal(x => x.Password).WithMessage("Şifreler eşleşmiyor");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Rol seçimi zorunludur")
            .Must(role => ValidRoles.Contains(role)).WithMessage("Geçersiz rol. Geçerli roller: Admin, Teacher, Student, Parent, Coach");
    }
}
