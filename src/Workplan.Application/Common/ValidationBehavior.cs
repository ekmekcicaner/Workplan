using FluentValidation;
using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Common;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result, IFailable<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(message);
            var failures = _validators
                .Select(v => v.Validate(context))
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .Select(f => f.ErrorMessage)
                .ToList();

            if (failures.Count != 0)
            {
                var error = Error.Validation("Doğrulama hatası.", failures);
                return TResponse.Fail(error);
            }
        }

        return await next(message, cancellationToken);
    }
}
