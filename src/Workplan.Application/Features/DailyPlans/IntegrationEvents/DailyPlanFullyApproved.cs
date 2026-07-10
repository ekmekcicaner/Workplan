using Workplan.Application.Common.Messaging;

namespace Workplan.Application.Features.DailyPlans.IntegrationEvents;

public sealed record DailyPlanFullyApproved(
    Guid EventId,
    DateTime OccurredOnUtc,
    Guid DailyPlanId) : IIntegrationEvent
{
    public const string EventName = "daily-plan.fully-approved.v1";
}
