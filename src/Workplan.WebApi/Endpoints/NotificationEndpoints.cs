using Mediator;
using Workplan.Application.Features.Notifications.Commands;
using Workplan.Application.Features.Notifications.Queries;
using Workplan.WebApi.Common;

namespace Workplan.WebApi.Endpoints;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/notifications").WithTags("Notifications")
            .RequireAuthorization();

        group.MapGet("/unread", async (ISender sender, CancellationToken ct)
            => (await sender.Send(new GetUnreadNotificationsQuery(), ct)).ToApiResult());

        group.MapPost("/{id:guid}/read", async (Guid id, ISender sender, CancellationToken ct)
            => (await sender.Send(new MarkNotificationReadCommand(id), ct)).ToApiResult());

        group.MapPost("/daily-plan/{dailyPlanId:guid}/read", async (Guid dailyPlanId, ISender sender, CancellationToken ct)
            => (await sender.Send(new MarkDailyPlanNotificationsReadCommand(dailyPlanId), ct)).ToApiResult());

        return app;
    }
}
