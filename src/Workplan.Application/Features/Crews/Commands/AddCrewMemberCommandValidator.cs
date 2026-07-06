using FluentValidation;

namespace Workplan.Application.Features.Crews.Commands;

public class AddCrewMemberCommandValidator : AbstractValidator<AddCrewMemberCommand>
{
    public AddCrewMemberCommandValidator()
    {
        RuleFor(x => x.CrewId).NotEmpty();
        RuleFor(x => x.PersonnelRef).NotEmpty().MaximumLength(100);
        RuleFor(x => x.WorkerType).IsInEnum();
    }
}
