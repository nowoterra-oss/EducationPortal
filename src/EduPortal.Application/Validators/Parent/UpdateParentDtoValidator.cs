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
            .Matches(@"^(\+90|0)?[0-9]{10}$").WithMessage("Geçerli bir telefon numarası giriniz")
            .When(x => !string.IsNullOrEmpty(x.WorkPhone));
    }
}
