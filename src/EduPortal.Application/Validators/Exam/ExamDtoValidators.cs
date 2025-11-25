using EduPortal.Application.DTOs.Exam;
using FluentValidation;

namespace EduPortal.Application.Validators.Exam;

public class InternalExamCreateDtoValidator : AbstractValidator<InternalExamCreateDto>
{
    public InternalExamCreateDtoValidator()
    {
        RuleFor(x => x.ExamName)
            .NotEmpty().WithMessage("Sınav adı zorunludur")
            .MaximumLength(200).WithMessage("Sınav adı en fazla 200 karakter olabilir");

        RuleFor(x => x.ExamDate)
            .NotEmpty().WithMessage("Sınav tarihi zorunludur");

        RuleFor(x => x.Duration)
            .GreaterThan(0).WithMessage("Süre 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(480).WithMessage("Sınav süresi 8 saatten fazla olamaz")
            .When(x => x.Duration.HasValue);

        RuleFor(x => x.TotalPoints)
            .GreaterThan(0).WithMessage("Toplam puan 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(1000).WithMessage("Toplam puan 1000'den fazla olamaz")
            .When(x => x.TotalPoints.HasValue);

        RuleFor(x => x.PassingScore)
            .GreaterThan(0).WithMessage("Geçme notu 0'dan büyük olmalıdır")
            .LessThan(x => x.TotalPoints ?? 100).WithMessage("Geçme notu toplam puandan az olmalıdır")
            .When(x => x.PassingScore.HasValue);

        RuleFor(x => x.CourseId)
            .GreaterThan(0).WithMessage("Geçerli bir ders ID giriniz")
            .When(x => x.CourseId.HasValue);

        RuleFor(x => x.ClassId)
            .GreaterThan(0).WithMessage("Geçerli bir sınıf ID giriniz")
            .When(x => x.ClassId.HasValue);

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Açıklama en fazla 1000 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Location)
            .MaximumLength(200).WithMessage("Yer en fazla 200 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Location));
    }
}

public class InternalExamUpdateDtoValidator : AbstractValidator<InternalExamUpdateDto>
{
    public InternalExamUpdateDtoValidator()
    {
        RuleFor(x => x.ExamName)
            .MaximumLength(200).WithMessage("Sınav adı en fazla 200 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.ExamName));

        RuleFor(x => x.Duration)
            .GreaterThan(0).WithMessage("Süre 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(480).WithMessage("Sınav süresi 8 saatten fazla olamaz")
            .When(x => x.Duration.HasValue);

        RuleFor(x => x.TotalPoints)
            .GreaterThan(0).WithMessage("Toplam puan 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(1000).WithMessage("Toplam puan 1000'den fazla olamaz")
            .When(x => x.TotalPoints.HasValue);

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Açıklama en fazla 1000 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Location)
            .MaximumLength(200).WithMessage("Yer en fazla 200 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Location));
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
            .LessThanOrEqualTo(1000).WithMessage("Puan 1000'den fazla olamaz");

        RuleFor(x => x.Comments)
            .MaximumLength(1000).WithMessage("Yorumlar en fazla 1000 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Comments));
    }
}
