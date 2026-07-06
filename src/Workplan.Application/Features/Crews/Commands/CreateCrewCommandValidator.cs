using FluentValidation;

namespace Workplan.Application.Features.Crews.Commands;

public class CreateCrewCommandValidator : AbstractValidator<CreateCrewCommand>
{
    public CreateCrewCommandValidator()
    {
        RuleFor(x => x.LocationId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}
