using FluentValidation;

namespace Workplan.Application.Features.CrewRegions.Commands;

public class CreateCrewRegionCommandValidator : AbstractValidator<CreateCrewRegionCommand>
{
    public CreateCrewRegionCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
