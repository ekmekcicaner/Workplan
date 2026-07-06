using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Features.DailyPlans;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Queries.GetDailyPlanByHeadMaster;

public class GetDailyPlanByHeadMasterQueryHandler
    : IRequestHandler<GetDailyPlanByHeadMasterQuery, Result<List<DailyPlanDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetDailyPlanByHeadMasterQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async ValueTask<Result<List<DailyPlanDto>>> Handle(
        GetDailyPlanByHeadMasterQuery request, CancellationToken cancellationToken)
    {
        var plans = await _db.DailyPlans
            .AsNoTracking()
            .Where(plan => plan.AssignedHoMId == request.HeadOfMasterUserId)
            .Select(plan => new DailyPlanDto(
                plan.Id, plan.ProjectId, plan.CrewRegionId, plan.LocationId, plan.WorkItemTypeId,
                plan.WorkDate, plan.PlannedById, plan.AssignedHoMId, plan.CrewId,
                plan.PlannedQuantity, plan.PlannedManDay, plan.Unit, plan.FactQuantity, plan.FactManDay,
                plan.Overtime, plan.Comment, plan.Status, null))
            .ToListAsync(cancellationToken);

        return plans;
    }
}
