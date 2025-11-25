using EduPortal.Application.DTOs.Student;
using FluentValidation;

namespace EduPortal.Application.Validators.Student;

public class StudentUpdateDtoValidator : AbstractValidator<StudentUpdateDto>
{
    public StudentUpdateDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage("Ad en fazla 100 karakter olabilir")
            .Matches(@"^[a-zA-ZçğıöşüÇĞİÖŞÜ\s]+$").WithMessage("Ad sadece harf içerebilir")
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Soyad en fazla 100 karakter olabilir")
            .Matches(@"^[a-zA-ZçğıöşüÇĞİÖŞÜ\s]+$").WithMessage("Soyad sadece harf içerebilir")
            .When(x => !string.IsNullOrEmpty(x.LastName));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz")
            .MaximumLength(256).WithMessage("Email en fazla 256 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^(\+90|0)?[0-9]{10}$").WithMessage("Geçerli bir telefon numarası giriniz")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.SchoolName)
            .MaximumLength(200).WithMessage("Okul adı en fazla 200 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.SchoolName));

        RuleFor(x => x.CurrentGrade)
            .InclusiveBetween(1, 12).WithMessage("Sınıf 1-12 arasında olmalıdır")
            .When(x => x.CurrentGrade.HasValue);

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Geçersiz cinsiyet değeri")
            .When(x => x.Gender.HasValue);

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Adres en fazla 500 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Address));

        RuleFor(x => x.LGSPercentile)
            .InclusiveBetween(0, 100).WithMessage("LGS yüzdelik dilimi 0-100 arasında olmalıdır")
            .When(x => x.LGSPercentile.HasValue);

        RuleFor(x => x.LanguageLevel)
            .MaximumLength(50).WithMessage("Dil seviyesi en fazla 50 karakter olabilir")
            .Matches(@"^(A1|A2|B1|B2|C1|C2|Native)?$").WithMessage("Geçerli dil seviyeleri: A1, A2, B1, B2, C1, C2, Native")
            .When(x => !string.IsNullOrEmpty(x.LanguageLevel));
    }
}
