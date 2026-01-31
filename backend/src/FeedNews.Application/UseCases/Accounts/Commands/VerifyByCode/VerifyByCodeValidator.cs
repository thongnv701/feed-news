using FluentValidation;

namespace FeedNews.Application.UseCases.Accounts.Commands.VerifyByCode;

public class VerifyByCodeValidator : AbstractValidator<VerifyByCodeCommand>
{
    public VerifyByCodeValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Code)
            .NotEmpty();
    }
}