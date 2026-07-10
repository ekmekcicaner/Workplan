using Mediator;
using Workplan.Application.Features.DailyPlans.IntegrationEvents;

namespace Workplan.Application.Features.DailyPlans.Notifications;

public sealed class DailyPlanFullyApprovedNotification : INotification
{
    public DailyPlanFullyApproved IntegrationEvent { get; }

    public DailyPlanFullyApprovedNotification(DailyPlanFullyApproved integrationEvent)
    {
        IntegrationEvent = integrationEvent;
    }
}
