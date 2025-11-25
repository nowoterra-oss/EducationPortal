using EduPortal.Application.DTOs.Exam;
using FluentValidation;

namespace EduPortal.Application.Validators.Exam;

public class InternalExamCreateDtoValidator : AbstractValidator<InternalExamCreateDto>
{
    public InternalExamCreateDtoValidator()
    {
        RuleFor(x => x.CourseId)
            .GreaterThan(0).WithMessage("Geçerli bir ders ID giriniz");

        RuleFor(x => x.ExamType)
            .NotEmpty().WithMessage("Sınav tipi zorunludur")
            .MaximumLength(50).WithMessage("Sınav tipi en fazla 50 karakter olabilir");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Başlık zorunludur")
            .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir");

        RuleFor(x => x.ExamDate)
            .NotEmpty().WithMessage("Sınav tarihi zorunludur");

        RuleFor(x => x.Duration)
            .InclusiveBetween(1, 300).WithMessage("Süre 1-300 dakika arasında olmalıdır")
            .When(x => x.Duration.HasValue);

        RuleFor(x => x.MaxScore)
            .InclusiveBetween(1, 1000).WithMessage("Maksimum puan 1-1000 arasında olmalıdır");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Açıklama en fazla 1000 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}

public class InternalExamUpdateDtoValidator : AbstractValidator<InternalExamUpdateDto>
{
    public InternalExamUpdateDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Geçerli bir sınav ID giriniz");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Başlık zorunludur")
            .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir");

        RuleFor(x => x.ExamDate)
            .NotEmpty().WithMessage("Sınav tarihi zorunludur");

        RuleFor(x => x.Duration)
            .InclusiveBetween(1, 300).WithMessage("Süre 1-300 dakika arasında olmalıdır")
            .When(x => x.Duration.HasValue);

        RuleFor(x => x.MaxScore)
            .InclusiveBetween(1, 1000).WithMessage("Maksimum puan 1-1000 arasında olmalıdır");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Açıklama en fazla 1000 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}

public class ExamResultCreateDtoValidator : AbstractValidator<ExamResultCreateDto>
{
    public ExamResultCreateDtoValidator()
    {
        RuleFor(x => x.ExamId)
            .GreaterThan(0).WithMessage("Geçerli bir sınav ID giriniz");

        RuleFor(x => x.StudentId)
            .GreaterThan(0).WithMessage("Geçerli bir öğrenci ID giriniz");

        RuleFor(x => x.Score)
            .GreaterThanOrEqualTo(0).WithMessage("Puan 0'dan küçük olamaz")
            .LessThanOrEqualTo(10000).WithMessage("Puan 10000'den fazla olamaz");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notlar en fazla 1000 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
