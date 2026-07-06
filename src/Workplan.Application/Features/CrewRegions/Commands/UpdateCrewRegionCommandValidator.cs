using FluentValidation;

namespace Workplan.Application.Features.CrewRegions.Commands;

public class UpdateCrewRegionCommandValidator : AbstractValidator<UpdateCrewRegionCommand>
{
    public UpdateCrewRegionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
