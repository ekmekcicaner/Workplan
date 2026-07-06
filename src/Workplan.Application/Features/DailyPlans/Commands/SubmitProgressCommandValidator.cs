using FluentValidation;

namespace Workplan.Application.Features.DailyPlans.Commands;

public class SubmitProgressCommandValidator : AbstractValidator<SubmitProgressCommand>
{
    public SubmitProgressCommandValidator()
    {
        RuleFor(x => x.DailyPlanId).NotEmpty();
    }
}
