using EduPortal.Application.DTOs.Email;
using FluentValidation;

namespace EduPortal.Application.Validators.Email;

public class SendEmailDtoValidator : AbstractValidator<SendEmailDto>
{
    public SendEmailDtoValidator()
    {
        RuleFor(x => x.To)
            .NotEmpty().WithMessage("Alıcı email adresi zorunludur")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz")
            .MaximumLength(256).WithMessage("Email en fazla 256 karakter olabilir");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Konu zorunludur")
            .MaximumLength(500).WithMessage("Konu en fazla 500 karakter olabilir");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("İçerik zorunludur")
            .MaximumLength(50000).WithMessage("İçerik en fazla 50000 karakter olabilir");

        RuleFor(x => x.Cc)
            .EmailAddress().WithMessage("Geçerli bir CC email adresi giriniz")
            .When(x => !string.IsNullOrEmpty(x.Cc));

        RuleFor(x => x.Bcc)
            .EmailAddress().WithMessage("Geçerli bir BCC email adresi giriniz")
            .When(x => !string.IsNullOrEmpty(x.Bcc));
    }
}

public class BulkEmailDtoValidator : AbstractValidator<BulkEmailDto>
{
    public BulkEmailDtoValidator()
    {
        RuleFor(x => x.Recipients)
            .NotEmpty().WithMessage("En az bir alıcı zorunludur")
            .Must(r => r.Count <= 100).WithMessage("Bir seferde en fazla 100 alıcıya email gönderilebilir");

        RuleForEach(x => x.Recipients)
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Konu zorunludur")
            .MaximumLength(500).WithMessage("Konu en fazla 500 karakter olabilir");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("İçerik zorunludur")
            .MaximumLength(50000).WithMessage("İçerik en fazla 50000 karakter olabilir");
    }
}

public class CreateEmailTemplateDtoValidator : AbstractValidator<CreateEmailTemplateDto>
{
    public CreateEmailTemplateDtoValidator()
    {
        RuleFor(x => x.TemplateName)
            .NotEmpty().WithMessage("Şablon adı zorunludur")
            .MaximumLength(200).WithMessage("Şablon adı en fazla 200 karakter olabilir");

        RuleFor(x => x.TemplateType)
            .GreaterThanOrEqualTo(0).WithMessage("Geçerli bir şablon tipi giriniz");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Konu zorunludur")
            .MaximumLength(300).WithMessage("Konu en fazla 300 karakter olabilir");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("İçerik zorunludur");

        RuleFor(x => x.VariablesJson)
            .MaximumLength(1000).WithMessage("Değişkenler en fazla 1000 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.VariablesJson));
    }
}
