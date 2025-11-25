using EduPortal.Application.DTOs.Branch;
using FluentValidation;

namespace EduPortal.Application.Validators.Branch;

public class UpdateBranchDtoValidator : AbstractValidator<UpdateBranchDto>
{
    public UpdateBranchDtoValidator()
    {
        RuleFor(x => x.BranchName)
            .MaximumLength(200).WithMessage("Şube adı en fazla 200 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.BranchName));

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Adres en fazla 500 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Address));

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("Şehir en fazla 100 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.City));

        RuleFor(x => x.District)
            .MaximumLength(100).WithMessage("İlçe en fazla 100 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.District));

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^(\+90|0)?[0-9]{10}$").WithMessage("Geçerli bir telefon numarası giriniz")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz")
            .MaximumLength(256).WithMessage("Email en fazla 256 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Kapasite 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(1000).WithMessage("Kapasite 1000'den fazla olamaz")
            .When(x => x.Capacity.HasValue);
    }
}
