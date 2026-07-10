using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Queries.GetDailyPlanById;

public class GetDailyPlanByIdQueryHandler(IApplicationDbContext db, IAccessScopeService accessScope)
    : IRequestHandler<GetDailyPlanByIdQuery, Result<DailyPlanDto>>
{
    public async ValueTask<Result<DailyPlanDto>> Handle(
        GetDailyPlanByIdQuery request, CancellationToken cancellationToken)
    {
        var exists = await db.DailyPlans
            .AsNoTracking()
            .AnyAsync(p => p.Id == request.Id, cancellationToken);
        if (!exists)
            return Result<DailyPlanDto>.Fail(Error.NotFound("Günlük plan bulunamadı."));

        var plan = await accessScope.ApplyDailyPlanScope(db.DailyPlans.AsNoTracking())
            .Include(p => p.History)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (plan is null)
            return Result<DailyPlanDto>.Fail(Error.ScopeMismatch("Bu günlük planı görüntüleme yetkiniz yok."));

        var history = plan.History
            .OrderBy(h => h.TransitionedAt)
            .Select(h => new StatusTransitionDto(h.FromStatus, h.ToStatus, h.ActionById, h.TransitionedAt, null, h.Note))
            .ToList();

        return new DailyPlanDto(
            plan.Id, plan.ProjectId, plan.CrewRegionId, plan.LocationId, plan.WorkItemTypeId,
            plan.WorkDate, plan.PlannedById, plan.AssignedHoMId, plan.CrewTypeId,
            plan.PlannedQuantity, plan.PlannedManDay, plan.Unit, plan.FactQuantity, plan.FactManDay,
            plan.Overtime, plan.Comment, plan.Status, history);
    }
}
