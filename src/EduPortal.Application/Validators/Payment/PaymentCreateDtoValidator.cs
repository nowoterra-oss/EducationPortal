using EduPortal.Application.DTOs.Payment;
using FluentValidation;

namespace EduPortal.Application.Validators.Payment;

public class PaymentCreateDtoValidator : AbstractValidator<PaymentCreateDto>
{
    public PaymentCreateDtoValidator()
    {
        RuleFor(x => x.StudentId)
            .GreaterThan(0).WithMessage("Geçerli bir öğrenci ID giriniz");

        RuleFor(x => x.InstallmentId)
            .GreaterThan(0).WithMessage("Geçerli bir taksit ID giriniz")
            .When(x => x.InstallmentId.HasValue);

        RuleFor(x => x.PaymentPlanId)
            .GreaterThan(0).WithMessage("Geçerli bir ödeme planı ID giriniz")
            .When(x => x.PaymentPlanId.HasValue);

        RuleFor(x => x.BranchId)
            .GreaterThan(0).WithMessage("Geçerli bir şube ID giriniz")
            .When(x => x.BranchId.HasValue);

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Tutar 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(1000000).WithMessage("Tutar 1.000.000 TL'den fazla olamaz");

        RuleFor(x => x.PaymentDate)
            .NotEmpty().WithMessage("Ödeme tarihi zorunludur")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("Ödeme tarihi gelecekte olamaz");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Geçersiz ödeme yöntemi");

        RuleFor(x => x.TransactionId)
            .MaximumLength(100).WithMessage("İşlem ID en fazla 100 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.TransactionId));

        RuleFor(x => x.ReceiptNumber)
            .MaximumLength(100).WithMessage("Makbuz numarası en fazla 100 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.ReceiptNumber));

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notlar en fazla 1000 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
