using FluentValidation;
using FeedNews.Application.Common.Abstractions.Messaging;
using FeedNews.Share.Model.Response;

namespace FeedNews.Application.UseCases.Test.Commands.TestValidateError;

public class TestValidateErrorCommand : ICommand<Result>
{
    public int Id { get; set; }

    public required string Email { get; set; }
}

public class TestValidator : AbstractValidator<TestValidateErrorCommand>
{
    public TestValidator()
    {
        RuleFor(x => x.Id)
            .LessThan(0)
            .WithMessage("số không thể nho hon 1");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email khong thể trống");
    }
}