using EduPortal.Application.DTOs.Student;
using EduPortal.Domain.Enums;
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

        // Global telefon numarası validasyonu: +[ülke kodu][numara] formatı
        // Örnek: +905321234567, +15551234567, +447911123456
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+[1-9][0-9]{6,14}$").WithMessage("Geçerli bir telefon numarası giriniz (örn: +905321234567)")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        // StudentNo backend tarafından otomatik oluşturulacak, validasyon gereksiz

        // Kimlik türü validasyonu
        RuleFor(x => x.IdentityType)
            .IsInEnum().WithMessage("Geçersiz kimlik türü");

        // Kimlik numarası validasyonu
        RuleFor(x => x.IdentityNumber)
            .NotEmpty().WithMessage("Kimlik numarası zorunludur")
            .MaximumLength(50).WithMessage("Kimlik numarası en fazla 50 karakter olabilir");

        // TC Kimlik için özel validasyon: 11 hane, sadece rakam, ilk hane 0 olamaz
        RuleFor(x => x.IdentityNumber)
            .Matches(@"^[1-9][0-9]{10}$").WithMessage("TC Kimlik numarası 11 haneli olmalı ve ilk hane 0 olamaz")
            .Must(BeValidTCKimlik).WithMessage("Geçersiz TC Kimlik numarası")
            .When(x => x.IdentityType == IdentityType.TCKimlik);

        // Pasaport için validasyon: alfanümerik, 6-20 karakter
        RuleFor(x => x.IdentityNumber)
            .Matches(@"^[A-Z0-9]{6,20}$").WithMessage("Pasaport numarası 6-20 karakter olmalı ve sadece büyük harf ve rakam içermelidir")
            .When(x => x.IdentityType == IdentityType.Pasaport);

        // Yabancı kimlik için validasyon: 5-20 karakter alfanümerik
        RuleFor(x => x.IdentityNumber)
            .Matches(@"^[A-Z0-9]{5,20}$").WithMessage("Yabancı kimlik numarası 5-20 karakter olmalıdır")
            .When(x => x.IdentityType == IdentityType.YabanciKimlik);

        // Uyruk validasyonu
        RuleFor(x => x.Nationality)
            .MaximumLength(100).WithMessage("Uyruk en fazla 100 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Nationality));

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

    /// <summary>
    /// TC Kimlik numarası algoritma kontrolü
    /// </summary>
    private static bool BeValidTCKimlik(string tcKimlik)
    {
        if (string.IsNullOrEmpty(tcKimlik) || tcKimlik.Length != 11)
            return false;

        if (!tcKimlik.All(char.IsDigit))
            return false;

        // İlk hane 0 olamaz
        if (tcKimlik[0] == '0')
            return false;

        int[] digits = tcKimlik.Select(c => c - '0').ToArray();

        // 10. hane kontrolü: (1,3,5,7,9. hanelerin toplamı * 7 - 2,4,6,8. hanelerin toplamı) mod 10
        int oddSum = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
        int evenSum = digits[1] + digits[3] + digits[5] + digits[7];
        int digit10 = (oddSum * 7 - evenSum) % 10;
        if (digit10 < 0) digit10 += 10;

        if (digits[9] != digit10)
            return false;

        // 11. hane kontrolü: ilk 10 hanenin toplamı mod 10
        int sumFirst10 = digits.Take(10).Sum();
        if (digits[10] != sumFirst10 % 10)
            return false;

        return true;
    }
}
