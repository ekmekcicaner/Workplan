using Workplan.Domain.Common;

namespace Workplan.Domain.Events;

public sealed class DailyWorkApprovedFullyEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredOnUtc { get; }
    public Guid DailyPlanId { get; }

    public DailyWorkApprovedFullyEvent(
        Guid dailyPlanId,
        Guid? eventId = null,
        DateTime? occurredOnUtc = null)
    {
        EventId = eventId ?? EntityId.New();
        OccurredOnUtc = occurredOnUtc ?? DateTime.UtcNow;
        DailyPlanId = dailyPlanId;
    }
}
