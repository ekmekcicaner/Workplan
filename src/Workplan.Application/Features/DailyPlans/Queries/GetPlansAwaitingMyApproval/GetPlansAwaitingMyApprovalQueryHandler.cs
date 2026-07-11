using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.Domain.Enums;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Queries.GetPlansAwaitingMyApproval;

public class GetPlansAwaitingMyApprovalQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IAccessScopeService accessScope)
    : IRequestHandler<GetPlansAwaitingMyApprovalQuery, Result<List<DailyPlanDto>>>
{
    public async ValueTask<Result<List<DailyPlanDto>>> Handle(
        GetPlansAwaitingMyApprovalQuery request, CancellationToken cancellationToken)
    {
        var roleResult = ApproverRoleMap.Resolve(currentUser.Roles);
        if (roleResult.IsFailure) return Result<List<DailyPlanDto>>.Fail(roleResult.Error);

        if (currentUser.UserId is not { } currentUserId)
            return Result<List<DailyPlanDto>>.Fail(Error.Unauthorized("Kimliği doğrulanmış bir kullanıcı gerekli."));

        var query = db.DailyPlans
            .AsNoTracking()
            .AsQueryable();

        query = accessScope.ApplyDailyPlanScope(query);

        query = roleResult.Value switch
        {
            WorkStatus.ApprovedBySiteChief => query.Where(p =>
                p.Status == WorkStatus.Submitted
                && db.CrewRegions.Any(r => r.Id == p.CrewRegionId && r.SiteChiefUserId == currentUserId)),
            WorkStatus.ApprovedByPM => query.Where(p =>
                p.Status == WorkStatus.ApprovedBySiteChief
                && db.Projects.Any(project => project.Id == p.ProjectId && project.PmUserId == currentUserId)),
            _ => query.Where(_ => false)
        };

        var plans = await query
            .Select(plan => new DailyPlanDto(
                plan.Id, plan.ProjectId, plan.CrewRegionId, plan.LocationId, plan.WorkItemTypeId,
                plan.WorkDate, plan.PlannedById, plan.AssignedHoMId, plan.CrewTypeId,
                plan.PlannedQuantity, plan.PlannedManDay, plan.Unit, plan.FactQuantity, plan.FactManDay,
                plan.Overtime, plan.Comment, plan.Status, null))
            .ToListAsync(cancellationToken);

        return plans;
    }
}
