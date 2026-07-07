using FluentValidation;

namespace Workplan.Application.Features.CrewTypes.Commands;

public class CreateCrewTypeCommandValidator : AbstractValidator<CreateCrewTypeCommand>
{
    public CreateCrewTypeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
