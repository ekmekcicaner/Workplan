using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.Domain.Entities;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Commands;

public class CreateDailyPlanCommandHandler : IRequestHandler<CreateDailyPlanCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreateDailyPlanCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async ValueTask<Result<Guid>> Handle(CreateDailyPlanCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } plannedById)
            return Result<Guid>.Fail(Error.Unauthorized("Kimliği doğrulanmış bir kullanıcı gerekli."));

        var locationValid = await _db.Locations.AsNoTracking()
            .AnyAsync(l => l.Id == request.LocationId
                           && l.CrewRegionId == request.CrewRegionId
                           && l.ProjectId == request.ProjectId, cancellationToken);
        if (!locationValid)
            return Result<Guid>.Fail(Error.NotFound("Lokasyon, belirtilen proje/bölgeye ait değil ya da bulunamadı."));

        var workItemType = await _db.WorkItemTypes.AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == request.WorkItemTypeId, cancellationToken);
        if (workItemType is null)
            return Result<Guid>.Fail(Error.NotFound("İş tipi (ToW/SToW/SSToW) bulunamadı."));
        if (workItemType.Level != 2)
            return Result<Guid>.Fail(Error.Validation("Planlama en alt seviye iş tipi (SSToW) üzerinden yapılmalıdır."));
        if (workItemType.Unit == Domain.ValueObjects.Unit.None)
            return Result<Guid>.Fail(Error.Validation("Seçilen iş kalemi tipi için birim tanımlanmamış."));

        var result = DailyPlan.CreateFromPlan(
            request.ProjectId, request.CrewRegionId, request.LocationId, request.WorkItemTypeId,
            request.WorkDate, plannedById, request.AssignedHoMId,
            request.PlannedQuantity, request.PlannedManDay, workItemType.Unit);
        if (result.IsFailure) return Result<Guid>.Fail(result.Error);

        _db.DailyPlans.Add(result.Value);
        var notification = Notification.CreateDailyPlanAssigned(
            request.AssignedHoMId, result.Value.Id, request.WorkDate);
        if (notification.IsFailure) return Result<Guid>.Fail(notification.Error);

        _db.Notifications.Add(notification.Value);
        await _db.SaveChangesAsync(cancellationToken);

        return result.Value.Id;
    }
}
