using Mediator;

namespace Workplan.Domain.Events;

public class DailyWorkApprovedFullyEvent : INotification
{
    public Guid DailyPlanId { get; }

    public DailyWorkApprovedFullyEvent(Guid dailyPlanId)
    {
        DailyPlanId = dailyPlanId;
    }
}