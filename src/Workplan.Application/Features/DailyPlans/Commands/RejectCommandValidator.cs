using FluentValidation;

namespace Workplan.Application.Features.DailyPlans.Commands;

public class RejectCommandValidator : AbstractValidator<RejectCommand>
{
    public RejectCommandValidator()
    {
        RuleFor(x => x.DailyPlanId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty();
    }
}
