using FluentValidation;

namespace Workplan.Application.Features.CrewTypes.Commands;

public class UpdateCrewTypeCommandValidator : AbstractValidator<UpdateCrewTypeCommand>
{
    public UpdateCrewTypeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
