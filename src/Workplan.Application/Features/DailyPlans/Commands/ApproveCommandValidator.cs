using FluentValidation;

namespace Workplan.Application.Features.DailyPlans.Commands;

public class ApproveCommandValidator : AbstractValidator<ApproveCommand>
{
    public ApproveCommandValidator()
    {
        RuleFor(x => x.DailyPlanId).NotEmpty();
    }
}
