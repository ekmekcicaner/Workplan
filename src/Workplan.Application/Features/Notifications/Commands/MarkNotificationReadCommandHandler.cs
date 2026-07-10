using Mediator;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Notifications.Commands;

public class MarkNotificationReadCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<MarkNotificationReadCommand, Result>
{
    public async ValueTask<Result> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
            return Result.Fail(Error.Unauthorized("Kimliği doğrulanmış bir kullanıcı gerekli."));

        var notification = await db.Notifications.FindAsync([request.NotificationId], cancellationToken);
        if (notification is null) return Result.Fail(Error.NotFound("Bildirim bulunamadı."));

        var result = notification.MarkAsRead(userId);
        if (result.IsFailure) return result;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
