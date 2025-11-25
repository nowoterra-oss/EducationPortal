using EduPortal.Application.DTOs.Coach;
using FluentValidation;

namespace EduPortal.Application.Validators.Coach;

public class UpdateCoachDtoValidator : AbstractValidator<UpdateCoachDto>
{
    public UpdateCoachDtoValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0).WithMessage("Geçerli bir şube ID giriniz")
            .When(x => x.BranchId.HasValue);

        RuleFor(x => x.Specialization)
            .NotEmpty().WithMessage("Uzmanlık alanı zorunludur")
            .MaximumLength(200).WithMessage("Uzmanlık alanı en fazla 200 karakter olabilir");

        RuleFor(x => x.Qualifications)
            .MaximumLength(1000).WithMessage("Nitelikler en fazla 1000 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Qualifications));

        RuleFor(x => x.ExperienceYears)
            .InclusiveBetween(0, 50).WithMessage("Deneyim yılı 0-50 arasında olmalıdır");

        RuleFor(x => x.HourlyRate)
            .GreaterThan(0).WithMessage("Saatlik ücret 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(10000).WithMessage("Saatlik ücret 10.000 TL'den fazla olamaz")
            .When(x => x.HourlyRate.HasValue);

        RuleFor(x => x.Bio)
            .MaximumLength(500).WithMessage("Biyografi en fazla 500 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Bio));
    }
}
