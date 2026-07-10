using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Notifications.Commands;

public class MarkDailyPlanNotificationsReadCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<MarkDailyPlanNotificationsReadCommand, Result>
{
    public async ValueTask<Result> Handle(
        MarkDailyPlanNotificationsReadCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
            return Result.Fail(Error.Unauthorized("Kimliği doğrulanmış bir kullanıcı gerekli."));

        var notifications = await db.Notifications
            .Where(n => n.UserId == userId
                        && n.DailyPlanId == request.DailyPlanId
                        && n.ReadAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var notification in notifications)
        {
            var result = notification.MarkAsRead(userId);
            if (result.IsFailure) return result;
        }

        await db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
