using Mediator;
using Microsoft.Extensions.Logging;
using Workplan.Domain.Events;

namespace Workplan.Application.Features.DailyPlans.Notifications;

// Raporlama seam'i: Daily Report konsolidasyonu/Power BI tetiklemesi gerektiğinde bu handler genişletilir.
public class DailyWorkApprovedFullyEventHandler : INotificationHandler<DailyWorkApprovedFullyEvent>
{
    private readonly ILogger<DailyWorkApprovedFullyEventHandler> _logger;

    public DailyWorkApprovedFullyEventHandler(ILogger<DailyWorkApprovedFullyEventHandler> logger)
    {
        _logger = logger;
    }

    public ValueTask Handle(DailyWorkApprovedFullyEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Günlük plan {DailyPlanId} tüm onay adımlarını (Site Chief -> PM) tamamladı.",
            notification.DailyPlanId);

        return ValueTask.CompletedTask;
    }
}
