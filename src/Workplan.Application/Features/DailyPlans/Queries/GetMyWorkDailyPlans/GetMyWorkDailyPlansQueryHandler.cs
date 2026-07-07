using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Queries.GetMyWorkDailyPlans;

public class GetMyWorkDailyPlansQueryHandler
    : IRequestHandler<GetMyWorkDailyPlansQuery, Result<List<DailyPlanListItemDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IAccessScopeService _accessScope;

    public GetMyWorkDailyPlansQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IAccessScopeService accessScope)
    {
        _db = db;
        _currentUser = currentUser;
        _accessScope = accessScope;
    }

    public async ValueTask<Result<List<DailyPlanListItemDto>>> Handle(
        GetMyWorkDailyPlansQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } currentUserId)
            return Result<List<DailyPlanListItemDto>>.Fail(Error.Unauthorized("Kimliği doğrulanmış bir kullanıcı gerekli."));

        var query = _accessScope
            .ApplyDailyPlanScope(_db.DailyPlans.AsNoTracking())
            .Where(plan => plan.AssignedHoMId == currentUserId);

        var plans = await (
            from plan in query
            join project in _db.Projects.AsNoTracking() on plan.ProjectId equals project.Id
            join region in _db.CrewRegions.AsNoTracking() on plan.CrewRegionId equals region.Id
            join location in _db.Locations.AsNoTracking() on plan.LocationId equals location.Id
            join workItemType in _db.WorkItemTypes.AsNoTracking() on plan.WorkItemTypeId equals workItemType.Id
            join crewType in _db.CrewTypes.AsNoTracking() on plan.CrewTypeId equals (Guid?)crewType.Id into crewTypeJoin
            from crewType in crewTypeJoin.DefaultIfEmpty()
            orderby plan.WorkDate descending, plan.Id
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
                                      && history.ToStatus == Workplan.Domain.Enums.WorkStatus.InProgress
                                      && (history.FromStatus == Workplan.Domain.Enums.WorkStatus.Submitted
                                          || history.FromStatus == Workplan.Domain.Enums.WorkStatus.ApprovedByHoM)
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
                                      && history.ToStatus == Workplan.Domain.Enums.WorkStatus.InProgress
                                      && (history.FromStatus == Workplan.Domain.Enums.WorkStatus.Submitted
                                          || history.FromStatus == Workplan.Domain.Enums.WorkStatus.ApprovedByHoM)
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
                                      && history.ToStatus == Workplan.Domain.Enums.WorkStatus.InProgress
                                      && (history.FromStatus == Workplan.Domain.Enums.WorkStatus.Submitted
                                          || history.FromStatus == Workplan.Domain.Enums.WorkStatus.ApprovedByHoM)
                                      && history.Note != null
                                      && history.Note != "")
                    .OrderByDescending(history => history.TransitionedAt)
                    .Select(history => (Workplan.Domain.Enums.WorkStatus?)history.FromStatus)
                    .FirstOrDefault()))
            .ToListAsync(cancellationToken);

        return plans;
    }
}
