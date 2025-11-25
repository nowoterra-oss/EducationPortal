using EduPortal.Application.DTOs.Homework;
using FluentValidation;

namespace EduPortal.Application.Validators.Homework;

public class HomeworkCreateDtoValidator : AbstractValidator<HomeworkCreateDto>
{
    public HomeworkCreateDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Ödev başlığı zorunludur")
            .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Açıklama en fazla 2000 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("Son teslim tarihi zorunludur")
            .GreaterThan(DateTime.UtcNow).WithMessage("Son teslim tarihi gelecekte olmalıdır");

        RuleFor(x => x.MaxScore)
            .GreaterThan(0).WithMessage("Maksimum puan 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(100).WithMessage("Maksimum puan 100'den fazla olamaz")
            .When(x => x.MaxScore.HasValue);

        RuleFor(x => x.CourseId)
            .GreaterThan(0).WithMessage("Geçerli bir ders ID giriniz")
            .When(x => x.CourseId.HasValue);

        RuleFor(x => x.ClassId)
            .GreaterThan(0).WithMessage("Geçerli bir sınıf ID giriniz")
            .When(x => x.ClassId.HasValue);
    }
}

public class HomeworkUpdateDtoValidator : AbstractValidator<HomeworkUpdateDto>
{
    public HomeworkUpdateDtoValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Açıklama en fazla 2000 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Son teslim tarihi gelecekte olmalıdır")
            .When(x => x.DueDate.HasValue);

        RuleFor(x => x.MaxScore)
            .GreaterThan(0).WithMessage("Maksimum puan 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(100).WithMessage("Maksimum puan 100'den fazla olamaz")
            .When(x => x.MaxScore.HasValue);
    }
}

public class HomeworkSubmitDtoValidator : AbstractValidator<HomeworkSubmitDto>
{
    public HomeworkSubmitDtoValidator()
    {
        RuleFor(x => x.HomeworkId)
            .GreaterThan(0).WithMessage("Geçerli bir ödev ID giriniz");

        RuleFor(x => x.StudentId)
            .GreaterThan(0).WithMessage("Geçerli bir öğrenci ID giriniz");

        RuleFor(x => x.SubmissionText)
            .MaximumLength(10000).WithMessage("Teslim metni en fazla 10000 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.SubmissionText));

        RuleFor(x => x.FileUrl)
            .MaximumLength(500).WithMessage("Dosya URL en fazla 500 karakter olabilir")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _)).WithMessage("Geçerli bir URL giriniz")
            .When(x => !string.IsNullOrEmpty(x.FileUrl));
    }
}

public class GradeSubmissionDtoValidator : AbstractValidator<GradeSubmissionDto>
{
    public GradeSubmissionDtoValidator()
    {
        RuleFor(x => x.SubmissionId)
            .GreaterThan(0).WithMessage("Geçerli bir teslim ID giriniz");

        RuleFor(x => x.Score)
            .GreaterThanOrEqualTo(0).WithMessage("Puan 0'dan küçük olamaz")
            .LessThanOrEqualTo(100).WithMessage("Puan 100'den fazla olamaz");

        RuleFor(x => x.Feedback)
            .MaximumLength(2000).WithMessage("Geri bildirim en fazla 2000 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Feedback));
    }
}
