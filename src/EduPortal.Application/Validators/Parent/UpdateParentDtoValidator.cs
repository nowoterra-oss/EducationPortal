using EduPortal.Application.DTOs.Parent;
using FluentValidation;

namespace EduPortal.Application.Validators.Parent;

public class UpdateParentDtoValidator : AbstractValidator<UpdateParentDto>
{
    public UpdateParentDtoValidator()
    {
        RuleFor(x => x.Occupation)
            .MaximumLength(200).WithMessage("Meslek en fazla 200 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Occupation));

        RuleFor(x => x.WorkPhone)
            .MaximumLength(20).WithMessage("İş telefonu en fazla 20 karakter olabilir")
            .Matches(@"^[\d\s\+\-\(\)]+$").WithMessage("Geçerli bir telefon numarası giriniz")
            .When(x => !string.IsNullOrEmpty(x.WorkPhone));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Telefon numarası en fazla 20 karakter olabilir")
            .Matches(@"^[\d\s\+\-\(\)]+$").WithMessage("Geçerli bir telefon numarası giriniz")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage("Ad en fazla 100 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Soyad en fazla 100 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.LastName));

        RuleFor(x => x.IdentityNumber)
            .MaximumLength(50).WithMessage("Kimlik numarası en fazla 50 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.IdentityNumber));

        RuleFor(x => x.Nationality)
            .MaximumLength(100).WithMessage("Uyruk en fazla 100 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Nationality));

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("İl en fazla 100 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.City));

        RuleFor(x => x.District)
            .MaximumLength(100).WithMessage("İlçe en fazla 100 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.District));

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Adres en fazla 500 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Address));
    }
}
