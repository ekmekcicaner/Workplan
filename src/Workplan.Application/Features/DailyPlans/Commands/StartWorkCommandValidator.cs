using FluentValidation;

namespace Workplan.Application.Features.DailyPlans.Commands;

public class StartWorkCommandValidator : AbstractValidator<StartWorkCommand>
{
    public StartWorkCommandValidator()
    {
        RuleFor(x => x.DailyPlanId).NotEmpty();
        RuleFor(x => x.CrewId).NotEmpty();
    }
}
