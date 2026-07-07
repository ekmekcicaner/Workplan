using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.Domain.Enums;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Queries.GetApprovalQueue;

public class GetApprovalQueueQueryHandler
    : IRequestHandler<GetApprovalQueueQuery, Result<List<DailyPlanListItemDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IAccessScopeService _accessScope;

    public GetApprovalQueueQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IAccessScopeService accessScope)
    {
        _db = db;
        _currentUser = currentUser;
        _accessScope = accessScope;
    }

    public async ValueTask<Result<List<DailyPlanListItemDto>>> Handle(
        GetApprovalQueueQuery request,
        CancellationToken cancellationToken)
    {
        var roleResult = ApproverRoleMap.Resolve(_currentUser.Roles);
        if (roleResult.IsFailure)
            return Result<List<DailyPlanListItemDto>>.Fail(roleResult.Error);

        if (_currentUser.UserId is not { } currentUserId)
            return Result<List<DailyPlanListItemDto>>.Fail(Error.Unauthorized("Kimliği doğrulanmış bir kullanıcı gerekli."));

        var query = _accessScope.ApplyDailyPlanScope(_db.DailyPlans.AsNoTracking());
        query = roleResult.Value switch
        {
            WorkStatus.ApprovedBySiteChief => query.Where(plan =>
                (plan.Status == WorkStatus.Submitted || plan.Status == WorkStatus.ApprovedByHoM)
                && _db.CrewRegions.Any(region => region.Id == plan.CrewRegionId && region.SiteChiefUserId == currentUserId)),
            WorkStatus.ApprovedByPM => query.Where(plan =>
                plan.Status == WorkStatus.ApprovedBySiteChief
                && _db.Projects.Any(project => project.Id == plan.ProjectId && project.PmUserId == currentUserId)),
            _ => query.Where(_ => false)
        };

        var plans = await (
            from plan in query
            join project in _db.Projects.AsNoTracking() on plan.ProjectId equals project.Id
            join region in _db.CrewRegions.AsNoTracking() on plan.CrewRegionId equals region.Id
            join location in _db.Locations.AsNoTracking() on plan.LocationId equals location.Id
            join workItemType in _db.WorkItemTypes.AsNoTracking() on plan.WorkItemTypeId equals workItemType.Id
            join crewType in _db.CrewTypes.AsNoTracking() on plan.CrewTypeId equals (Guid?)crewType.Id into crewTypeJoin
            from crewType in crewTypeJoin.DefaultIfEmpty()
            orderby plan.WorkDate, plan.Id
            select new DailyPlanListItemDto(
                plan.Id,
                plan.ProjectId,
                project.Name,
                plan.CrewRegionId,
                region.Name,
                plan.LocationId,
                location.Name,
                plan.WorkItemTypeId,
                workItemType.Name,
                plan.WorkDate,
                plan.PlannedById,
                plan.AssignedHoMId,
                plan.CrewTypeId,
                crewType == null ? null : crewType.Name,
                plan.PlannedQuantity,
                plan.PlannedManDay,
                plan.Unit,
                plan.FactQuantity,
                plan.FactManDay,
                plan.Overtime,
                plan.Comment,
                plan.Status,
                plan.History
                    .Where(history => history.ToStatus == plan.Status
                                      && history.TransitionedAt == plan.History
                                          .OrderByDescending(lastHistory => lastHistory.TransitionedAt)
                                          .Select(lastHistory => lastHistory.TransitionedAt)
                                          .FirstOrDefault()
                                      && history.ToStatus == WorkStatus.Submitted
                                      && history.FromStatus == WorkStatus.ApprovedBySiteChief
                                      && history.Note != null
                                      && history.Note != "")
                    .OrderByDescending(history => history.TransitionedAt)
                    .Select(history => history.Note)
                    .FirstOrDefault(),
                plan.History
                    .Where(history => history.ToStatus == plan.Status
                                      && history.TransitionedAt == plan.History
                                          .OrderByDescending(lastHistory => lastHistory.TransitionedAt)
                                          .Select(lastHistory => lastHistory.TransitionedAt)
                                          .FirstOrDefault()
                                      && history.ToStatus == WorkStatus.Submitted
                                      && history.FromStatus == WorkStatus.ApprovedBySiteChief
                                      && history.Note != null
                                      && history.Note != "")
                    .OrderByDescending(history => history.TransitionedAt)
                    .Select(history => (DateTime?)history.TransitionedAt)
                    .FirstOrDefault(),
                plan.History
                    .Where(history => history.ToStatus == plan.Status
                                      && history.TransitionedAt == plan.History
                                          .OrderByDescending(lastHistory => lastHistory.TransitionedAt)
                                          .Select(lastHistory => lastHistory.TransitionedAt)
                                          .FirstOrDefault()
                                      && history.ToStatus == WorkStatus.Submitted
                                      && history.FromStatus == WorkStatus.ApprovedBySiteChief
                                      && history.Note != null
                                      && history.Note != "")
                    .OrderByDescending(history => history.TransitionedAt)
                    .Select(history => (WorkStatus?)history.FromStatus)
                    .FirstOrDefault()))
            .ToListAsync(cancellationToken);

        return plans;
    }
}
