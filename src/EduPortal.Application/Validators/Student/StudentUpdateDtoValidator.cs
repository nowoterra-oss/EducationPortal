using EduPortal.Application.DTOs.Student;
using FluentValidation;

namespace EduPortal.Application.Validators.Student;

public class StudentUpdateDtoValidator : AbstractValidator<StudentUpdateDto>
{
    public StudentUpdateDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Geçerli bir öğrenci ID giriniz");

        RuleFor(x => x.StudentNo)
            .MaximumLength(20).WithMessage("Öğrenci numarası en fazla 20 karakter olabilir")
            .Matches(@"^[A-Z0-9-]+$").WithMessage("Öğrenci numarası sadece büyük harf, rakam ve tire içerebilir")
            .When(x => !string.IsNullOrEmpty(x.StudentNo));

        RuleFor(x => x.SchoolName)
            .MaximumLength(200).WithMessage("Okul adı en fazla 200 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.SchoolName));

        RuleFor(x => x.CurrentGrade)
            .InclusiveBetween(1, 12).WithMessage("Sınıf 1-12 arasında olmalıdır")
            .When(x => x.CurrentGrade.HasValue);

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Geçersiz cinsiyet değeri")
            .When(x => x.Gender.HasValue);

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.UtcNow.AddYears(-5)).WithMessage("Öğrenci en az 5 yaşında olmalıdır")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Adres en fazla 500 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Address));

        RuleFor(x => x.LGSPercentile)
            .InclusiveBetween(0, 100).WithMessage("LGS yüzdelik dilimi 0-100 arasında olmalıdır")
            .When(x => x.LGSPercentile.HasValue);

        RuleFor(x => x.BilsemField)
            .MaximumLength(100).WithMessage("Bilsem alanı en fazla 100 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.BilsemField));

        RuleFor(x => x.LanguageLevel)
            .MaximumLength(50).WithMessage("Dil seviyesi en fazla 50 karakter olabilir")
            .Matches(@"^(A1|A2|B1|B2|C1|C2|Native)?$").WithMessage("Geçerli dil seviyeleri: A1, A2, B1, B2, C1, C2, Native")
            .When(x => !string.IsNullOrEmpty(x.LanguageLevel));

        RuleFor(x => x.TargetMajor)
            .MaximumLength(200).WithMessage("Hedef bölüm en fazla 200 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.TargetMajor));

        RuleFor(x => x.TargetCountry)
            .MaximumLength(100).WithMessage("Hedef ülke en fazla 100 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.TargetCountry));

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^(\+90|0)?[0-9]{10}$").WithMessage("Geçerli bir telefon numarası giriniz")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}
