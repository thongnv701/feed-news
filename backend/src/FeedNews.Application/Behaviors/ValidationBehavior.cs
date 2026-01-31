using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace FeedNews.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any()) return await next();

        ValidationContext<TRequest> context = new(request);

        ValidationResult[] validationResult = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        List<ValidationFailure> failures = validationResult
            .Where(r => r.Errors.Any())
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Any()) throw new ValidationException(failures);

        return await next();
    }
}