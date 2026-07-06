using FluentValidation;

namespace Workplan.Application.Features.DailyPlans.Commands;

public class CreateDailyPlanCommandValidator : AbstractValidator<CreateDailyPlanCommand>
{
    public CreateDailyPlanCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.CrewRegionId).NotEmpty();
        RuleFor(x => x.LocationId).NotEmpty();
        RuleFor(x => x.WorkItemTypeId).NotEmpty();
        RuleFor(x => x.AssignedHoMId).NotEmpty();
        RuleFor(x => x.PlannedQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PlannedManDay).GreaterThanOrEqualTo(0);
    }
}
