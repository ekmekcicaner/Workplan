using System.Text.Json;
using Workplan.Application.Common.Messaging;
using Workplan.Application.Features.DailyPlans.IntegrationEvents;
using Workplan.Domain.Common;
using Workplan.Domain.Events;

namespace Workplan.Infrastructure.Messaging.Outbox;

internal static class IntegrationEventSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static OutboxMessage CreateOutboxMessage(IDomainEvent domainEvent)
    {
        var integrationEvent = Map(domainEvent);
        var eventName = GetEventName(integrationEvent);
        var payload = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType(), JsonOptions);

        return OutboxMessage.Create(
            integrationEvent.EventId,
            eventName,
            payload,
            integrationEvent.OccurredOnUtc);
    }

    public static IIntegrationEvent Deserialize(OutboxMessage message) =>
        message.Type switch
        {
            DailyPlanFullyApproved.EventName =>
                JsonSerializer.Deserialize<DailyPlanFullyApproved>(message.Payload, JsonOptions)
                ?? throw new InvalidOperationException(
                    $"Outbox mesajı '{message.Id}' deserialize edilemedi."),
            _ => throw new InvalidOperationException(
                $"Bilinmeyen integration event tipi: '{message.Type}'.")
        };

    public static string GetEventName(IIntegrationEvent integrationEvent) =>
        integrationEvent switch
        {
            DailyPlanFullyApproved => DailyPlanFullyApproved.EventName,
            _ => throw new InvalidOperationException(
                $"'{integrationEvent.GetType().Name}' için event adı tanımlanmamış.")
        };

    private static IIntegrationEvent Map(IDomainEvent domainEvent) =>
        domainEvent switch
        {
            DailyWorkApprovedFullyEvent approved => new DailyPlanFullyApproved(
                approved.EventId,
                approved.OccurredOnUtc,
                approved.DailyPlanId),
            _ => throw new InvalidOperationException(
                $"'{domainEvent.GetType().Name}' için integration event eşlemesi tanımlanmamış.")
        };
}
