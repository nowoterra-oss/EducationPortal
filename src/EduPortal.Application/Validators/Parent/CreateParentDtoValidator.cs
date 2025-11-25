using EduPortal.Application.DTOs.Parent;
using FluentValidation;

namespace EduPortal.Application.Validators.Parent;

public class CreateParentDtoValidator : AbstractValidator<CreateParentDto>
{
    public CreateParentDtoValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur");

        RuleFor(x => x.Occupation)
            .MaximumLength(200).WithMessage("Meslek en fazla 200 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Occupation));

        RuleFor(x => x.WorkPhone)
            .MaximumLength(20).WithMessage("İş telefonu en fazla 20 karakter olabilir")
            .Matches(@"^(\+90|0)?[0-9]{10}$").WithMessage("Geçerli bir telefon numarası giriniz")
            .When(x => !string.IsNullOrEmpty(x.WorkPhone));

        RuleForEach(x => x.StudentRelationships)
            .SetValidator(new StudentRelationshipDtoValidator())
            .When(x => x.StudentRelationships != null && x.StudentRelationships.Any());
    }
}

public class StudentRelationshipDtoValidator : AbstractValidator<StudentRelationshipDto>
{
    private static readonly string[] ValidRelationships = { "Anne", "Baba", "Vasi", "Dede", "Nine", "Amca", "Dayı", "Hala", "Teyze" };

    public StudentRelationshipDtoValidator()
    {
        RuleFor(x => x.StudentId)
            .GreaterThan(0).WithMessage("Geçerli bir öğrenci ID giriniz");

        RuleFor(x => x.Relationship)
            .NotEmpty().WithMessage("İlişki türü zorunludur")
            .MaximumLength(50).WithMessage("İlişki türü en fazla 50 karakter olabilir")
            .Must(r => ValidRelationships.Contains(r)).WithMessage($"Geçerli ilişki türleri: {string.Join(", ", ValidRelationships)}");
    }
}
