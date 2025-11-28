using EduPortal.Application.DTOs.Scheduling;
using FluentValidation;

namespace EduPortal.Application.Validators.Scheduling;

public class CreateLessonScheduleDtoValidator : AbstractValidator<CreateLessonScheduleDto>
{
    public CreateLessonScheduleDtoValidator()
    {
        RuleFor(x => x.StudentId)
            .GreaterThan(0)
            .WithMessage("Geçerli bir öğrenci seçiniz");

        RuleFor(x => x.TeacherId)
            .GreaterThan(0)
            .WithMessage("Geçerli bir öğretmen seçiniz");

        RuleFor(x => x.CourseId)
            .GreaterThan(0)
            .WithMessage("Geçerli bir ders seçiniz");

        RuleFor(x => x.DayOfWeek)
            .IsInEnum()
            .WithMessage("Geçersiz gün değeri. Pazar=0/7, Pazartesi=1, Salı=2, Çarşamba=3, Perşembe=4, Cuma=5, Cumartesi=6");

        RuleFor(x => x.StartTime)
            .NotEmpty()
            .WithMessage("Başlangıç saati gereklidir")
            .LessThan(x => x.EndTime)
            .WithMessage("Başlangıç saati bitiş saatinden önce olmalıdır");

        RuleFor(x => x.EndTime)
            .NotEmpty()
            .WithMessage("Bitiş saati gereklidir");

        RuleFor(x => x.EffectiveFrom)
            .NotEmpty()
            .WithMessage("Geçerlilik başlangıç tarihi gereklidir");

        RuleFor(x => x.EffectiveTo)
            .GreaterThan(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue)
            .WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır");
    }
}

public class CreateStudentAvailabilityDtoValidator : AbstractValidator<CreateStudentAvailabilityDto>
{
    public CreateStudentAvailabilityDtoValidator()
    {
        RuleFor(x => x.StudentId)
            .GreaterThan(0)
            .WithMessage("Geçerli bir öğrenci seçiniz");

        RuleFor(x => x.DayOfWeek)
            .IsInEnum()
            .WithMessage("Geçersiz gün değeri. Pazar=0/7, Pazartesi=1, Salı=2, Çarşamba=3, Perşembe=4, Cuma=5, Cumartesi=6");

        RuleFor(x => x.StartTime)
            .NotEmpty()
            .WithMessage("Başlangıç saati gereklidir")
            .LessThan(x => x.EndTime)
            .WithMessage("Başlangıç saati bitiş saatinden önce olmalıdır");

        RuleFor(x => x.EndTime)
            .NotEmpty()
            .WithMessage("Bitiş saati gereklidir");

        RuleFor(x => x.Type)
            .InclusiveBetween(0, 4)
            .WithMessage("Geçersiz müsaitlik tipi");
    }
}

public class CreateTeacherAvailabilityDtoValidator : AbstractValidator<CreateTeacherAvailabilityDto>
{
    public CreateTeacherAvailabilityDtoValidator()
    {
        RuleFor(x => x.TeacherId)
            .GreaterThan(0)
            .WithMessage("Geçerli bir öğretmen seçiniz");

        RuleFor(x => x.DayOfWeek)
            .IsInEnum()
            .WithMessage("Geçersiz gün değeri. Pazar=0/7, Pazartesi=1, Salı=2, Çarşamba=3, Perşembe=4, Cuma=5, Cumartesi=6");

        RuleFor(x => x.StartTime)
            .NotEmpty()
            .WithMessage("Başlangıç saati gereklidir")
            .LessThan(x => x.EndTime)
            .WithMessage("Başlangıç saati bitiş saatinden önce olmalıdır");

        RuleFor(x => x.EndTime)
            .NotEmpty()
            .WithMessage("Bitiş saati gereklidir");

        RuleFor(x => x.Type)
            .InclusiveBetween(0, 4)
            .WithMessage("Geçersiz müsaitlik tipi");
    }
}
