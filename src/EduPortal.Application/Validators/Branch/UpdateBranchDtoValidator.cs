using EduPortal.Application.DTOs.Branch;
using FluentValidation;

namespace EduPortal.Application.Validators.Branch;

public class UpdateBranchDtoValidator : AbstractValidator<UpdateBranchDto>
{
    public UpdateBranchDtoValidator()
    {
        RuleFor(x => x.BranchName)
            .NotEmpty().WithMessage("Şube adı zorunludur")
            .MaximumLength(200).WithMessage("Şube adı en fazla 200 karakter olabilir");

        RuleFor(x => x.Type)
            .InclusiveBetween(0, 2).WithMessage("Geçersiz şube tipi (0: Ana, 1: Şube, 2: Uydu)");

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Adres en fazla 500 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Address));

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("Şehir en fazla 100 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.City));

        RuleFor(x => x.District)
            .MaximumLength(100).WithMessage("İlçe en fazla 100 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.District));

        RuleFor(x => x.Phone)
            .Matches(@"^(\+90|0)?[0-9]{10}$").WithMessage("Geçerli bir telefon numarası giriniz")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz")
            .MaximumLength(100).WithMessage("Email en fazla 100 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Capacity)
            .InclusiveBetween(1, 10000).WithMessage("Kapasite 1-10000 arasında olmalıdır");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notlar en fazla 500 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
