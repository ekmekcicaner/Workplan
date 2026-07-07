using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Features.DailyPlans;
using Workplan.Application.Interfaces;
using Workplan.Domain.Enums;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Queries.GetApprovedDailyPlans;

public class GetApprovedDailyPlansQueryHandler
    : IRequestHandler<GetApprovedDailyPlansQuery, Result<List<DailyPlanDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly IAccessScopeService _accessScope;

    public GetApprovedDailyPlansQueryHandler(IApplicationDbContext db, IAccessScopeService accessScope)
    {
        _db = db;
        _accessScope = accessScope;
    }

    public async ValueTask<Result<List<DailyPlanDto>>> Handle(
        GetApprovedDailyPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = await _accessScope.ApplyDailyPlanScope(_db.DailyPlans
            .AsNoTracking()
            .Where(p => p.Status == WorkStatus.ApprovedByPM))
            .Select(plan => new DailyPlanDto(
                plan.Id, plan.ProjectId, plan.CrewRegionId, plan.LocationId, plan.WorkItemTypeId,
                plan.WorkDate, plan.PlannedById, plan.AssignedHoMId, plan.CrewTypeId,
                plan.PlannedQuantity, plan.PlannedManDay, plan.Unit, plan.FactQuantity, plan.FactManDay,
                plan.Overtime, plan.Comment, plan.Status, null))
            .ToListAsync(cancellationToken);

        return plans;
    }
}
