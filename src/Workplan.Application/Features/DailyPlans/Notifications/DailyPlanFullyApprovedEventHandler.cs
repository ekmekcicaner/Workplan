using Mediator;
using Microsoft.Extensions.Logging;

namespace Workplan.Application.Features.DailyPlans.Notifications;

// In-process consumer. Dış sistem teslimatları Infrastructure outbox dispatcher'ından yapılır.
public sealed class DailyPlanFullyApprovedEventHandler
    : INotificationHandler<DailyPlanFullyApprovedNotification>
{
    private readonly ILogger<DailyPlanFullyApprovedEventHandler> _logger;

    public DailyPlanFullyApprovedEventHandler(ILogger<DailyPlanFullyApprovedEventHandler> logger)
    {
        _logger = logger;
    }

    public ValueTask Handle(
        DailyPlanFullyApprovedNotification notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Günlük plan {DailyPlanId} tüm onay adımlarını (Site Chief -> PM) tamamladı. EventId: {EventId}",
            notification.IntegrationEvent.DailyPlanId,
            notification.IntegrationEvent.EventId);

        return ValueTask.CompletedTask;
    }
}
