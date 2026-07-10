using FluentValidation;
using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Common;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result, IFailable<TResponse>
{
    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any()) return await next(message, cancellationToken);
        var context = new ValidationContext<TRequest>(message);

        var failures = validators
            .Select(validator => validator.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .Select(failure => failure.ErrorMessage)
            .ToList();

        if (failures.Count is not 0)
        {
            return TResponse.Fail(
                Error.Validation("Doğrulama hatası.", failures));
        }

        return await next(message, cancellationToken);
    }
}