using Mediator;
using Workplan.Application.Common.Messaging;
using Workplan.Application.Features.DailyPlans.IntegrationEvents;
using Workplan.Application.Features.DailyPlans.Notifications;

namespace Workplan.Infrastructure.Messaging;

internal sealed class IntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly IPublisher _mediator;
    private readonly Webhooks.WebhookIntegrationEventPublisher _webhookPublisher;

    public IntegrationEventPublisher(
        IPublisher mediator,
        Webhooks.WebhookIntegrationEventPublisher webhookPublisher)
    {
        _mediator = mediator;
        _webhookPublisher = webhookPublisher;
    }

    public async Task PublishAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        // Dış teslimat başarısızsa outbox mesajı retry'a kalır. In-process handler'lar
        // ancak tüm yapılandırılmış dış teslimatlar başarılı olduktan sonra çalışır.
        await _webhookPublisher.PublishAsync(integrationEvent, cancellationToken);

        switch (integrationEvent)
        {
            case DailyPlanFullyApproved approved:
                await _mediator.Publish(
                    new DailyPlanFullyApprovedNotification(approved),
                    cancellationToken);
                break;
            default:
                throw new InvalidOperationException(
                    $"'{integrationEvent.GetType().Name}' için Mediator notification eşlemesi tanımlanmamış.");
        }
    }
}
