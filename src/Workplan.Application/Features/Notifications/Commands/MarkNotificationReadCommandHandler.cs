using Mediator;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Notifications.Commands;

public class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public MarkNotificationReadCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async ValueTask<Result> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
            return Result.Fail(Error.Unauthorized("Kimliği doğrulanmış bir kullanıcı gerekli."));

        var notification = await _db.Notifications.FindAsync([request.NotificationId], cancellationToken);
        if (notification is null) return Result.Fail(Error.NotFound("Bildirim bulunamadı."));

        var result = notification.MarkAsRead(userId);
        if (result.IsFailure) return result;

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
