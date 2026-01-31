using FluentValidation;

namespace FeedNews.Application.UseCases.Accounts.Commands.RegisterByEmail;

public class RegisterByEmailValidator : AbstractValidator<RegisterByEmailCommand>
{
    public RegisterByEmailValidator()
    {
        RuleFor(x => x.Input.Email)
            .EmailAddress();
        RuleFor(x => x.Input.Password)
            .NotEmpty();
        RuleFor(x => x.Input.ConfirmPassword)
            .NotEmpty()
            .Equal(x => x.Input.Password).WithMessage("Passwords do not match");
        RuleFor(x => x.Input.FullName)
            .NotEmpty();
    }
}