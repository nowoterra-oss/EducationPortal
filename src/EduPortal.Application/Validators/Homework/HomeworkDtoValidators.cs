using EduPortal.Application.DTOs.Homework;
using FluentValidation;

namespace EduPortal.Application.Validators.Homework;

public class HomeworkCreateDtoValidator : AbstractValidator<HomeworkCreateDto>
{
    public HomeworkCreateDtoValidator()
    {
        RuleFor(x => x.CourseId)
            .GreaterThan(0).WithMessage("Geçerli bir ders ID giriniz");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Ödev başlığı zorunludur")
            .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Açıklama en fazla 2000 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.AssignedDate)
            .NotEmpty().WithMessage("Atanma tarihi zorunludur");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("Son teslim tarihi zorunludur")
            .GreaterThan(x => x.AssignedDate).WithMessage("Son teslim tarihi atanma tarihinden sonra olmalıdır");

        RuleFor(x => x.MaxScore)
            .InclusiveBetween(0, 1000).WithMessage("Maksimum puan 0-1000 arasında olmalıdır")
            .When(x => x.MaxScore.HasValue);

        RuleFor(x => x.AttachmentUrl)
            .MaximumLength(500).WithMessage("Ek URL en fazla 500 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.AttachmentUrl));
    }
}

public class HomeworkUpdateDtoValidator : AbstractValidator<HomeworkUpdateDto>
{
    public HomeworkUpdateDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Geçerli bir ödev ID giriniz");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Başlık zorunludur")
            .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Açıklama en fazla 2000 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("Son teslim tarihi zorunludur");

        RuleFor(x => x.MaxScore)
            .InclusiveBetween(0, 1000).WithMessage("Maksimum puan 0-1000 arasında olmalıdır")
            .When(x => x.MaxScore.HasValue);

        RuleFor(x => x.AttachmentUrl)
            .MaximumLength(500).WithMessage("Ek URL en fazla 500 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.AttachmentUrl));
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

        RuleFor(x => x.SubmissionUrl)
            .MaximumLength(500).WithMessage("Teslim URL en fazla 500 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.SubmissionUrl));

        RuleFor(x => x.Comment)
            .MaximumLength(1000).WithMessage("Yorum en fazla 1000 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Comment));

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Geçersiz ödev durumu");
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

        RuleFor(x => x.TeacherFeedback)
            .MaximumLength(2000).WithMessage("Geri bildirim en fazla 2000 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.TeacherFeedback));
    }
}
