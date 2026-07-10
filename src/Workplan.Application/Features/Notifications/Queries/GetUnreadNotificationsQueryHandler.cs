using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Features.Notifications;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Notifications.Queries;

public class GetUnreadNotificationsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<GetUnreadNotificationsQuery, Result<List<NotificationDto>>>
{
    public async ValueTask<Result<List<NotificationDto>>> Handle(
        GetUnreadNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
            return Result<List<NotificationDto>>.Fail(Error.Unauthorized("Kimliği doğrulanmış bir kullanıcı gerekli."));

        var notifications = await db.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId && n.ReadAtUtc == null)
            .OrderByDescending(n => n.CreatedAtUtc)
            .Select(n => new NotificationDto(
                n.Id, n.Type, n.Title, n.Message, n.Link, n.DailyPlanId, n.CreatedAtUtc, n.ReadAtUtc))
            .ToListAsync(cancellationToken);

        return notifications;
    }
}
