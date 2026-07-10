using Mediator;
using Workplan.Application.Features.DailyPlans.IntegrationEvents;

namespace Workplan.Application.Features.DailyPlans.Notifications;

public sealed class DailyPlanFullyApprovedNotification(DailyPlanFullyApproved integrationEvent) : INotification
{
    public DailyPlanFullyApproved IntegrationEvent { get; } = integrationEvent;
}
