using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Notifications.Commands;

public class MarkDailyPlanNotificationsReadCommandHandler
    : IRequestHandler<MarkDailyPlanNotificationsReadCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public MarkDailyPlanNotificationsReadCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async ValueTask<Result> Handle(
        MarkDailyPlanNotificationsReadCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
            return Result.Fail(Error.Unauthorized("Kimliği doğrulanmış bir kullanıcı gerekli."));

        var notifications = await _db.Notifications
            .Where(n => n.UserId == userId
                        && n.DailyPlanId == request.DailyPlanId
                        && n.ReadAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var notification in notifications)
        {
            var result = notification.MarkAsRead(userId);
            if (result.IsFailure) return result;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
