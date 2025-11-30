using EduPortal.Application.DTOs.Student;
using FluentValidation;

namespace EduPortal.Application.Validators.Student;

public class StudentCreateDtoValidator : AbstractValidator<StudentCreateDto>
{
    public StudentCreateDtoValidator()
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
            .Matches(@"^(\+90|0)?[0-9]{10}$").WithMessage("Geçerli bir telefon numarası giriniz")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        // StudentNo backend tarafından otomatik oluşturulacak, validasyon gereksiz

        RuleFor(x => x.SchoolName)
            .NotEmpty().WithMessage("Okul adı zorunludur")
            .MaximumLength(200).WithMessage("Okul adı en fazla 200 karakter olabilir");

        RuleFor(x => x.CurrentGrade)
            .NotEmpty().WithMessage("Sınıf seviyesi zorunludur")
            .InclusiveBetween(1, 12).WithMessage("Sınıf 1-12 arasında olmalıdır");

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Geçersiz cinsiyet değeri");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Doğum tarihi zorunludur")
            .LessThan(DateTime.UtcNow.AddYears(-5)).WithMessage("Öğrenci en az 5 yaşında olmalıdır")
            .GreaterThan(DateTime.UtcNow.AddYears(-25)).WithMessage("Geçersiz doğum tarihi");

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

        RuleFor(x => x.ReferenceSource)
            .MaximumLength(200).WithMessage("Referans kaynağı en fazla 200 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.ReferenceSource));

        RuleFor(x => x.EnrollmentDate)
            .NotEmpty().WithMessage("Kayıt tarihi zorunludur")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(30)).WithMessage("Kayıt tarihi bugünden en fazla 30 gün sonrası olabilir");
    }
}
