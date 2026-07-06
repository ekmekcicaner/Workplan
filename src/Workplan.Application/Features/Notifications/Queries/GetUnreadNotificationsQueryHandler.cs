using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Features.Notifications;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Notifications.Queries;

public class GetUnreadNotificationsQueryHandler
    : IRequestHandler<GetUnreadNotificationsQuery, Result<List<NotificationDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetUnreadNotificationsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async ValueTask<Result<List<NotificationDto>>> Handle(
        GetUnreadNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
            return Result<List<NotificationDto>>.Fail(Error.Unauthorized("Kimliği doğrulanmış bir kullanıcı gerekli."));

        var notifications = await _db.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId && n.ReadAtUtc == null)
            .OrderByDescending(n => n.CreatedAtUtc)
            .Select(n => new NotificationDto(
                n.Id, n.Type, n.Title, n.Message, n.Link, n.DailyPlanId, n.CreatedAtUtc, n.ReadAtUtc))
            .ToListAsync(cancellationToken);

        return notifications;
    }
}
