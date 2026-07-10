using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Queries.GetDailyPlanDetail;

public class GetDailyPlanDetailQueryHandler(
    IApplicationDbContext db,
    IAccessScopeService accessScope,
    IIdentityService identityService)
    : IRequestHandler<GetDailyPlanDetailQuery, Result<DailyPlanDetailDto>>
{
    public async ValueTask<Result<DailyPlanDetailDto>> Handle(
        GetDailyPlanDetailQuery request,
        CancellationToken cancellationToken)
    {
        var exists = await db.DailyPlans
            .AsNoTracking()
            .AnyAsync(plan => plan.Id == request.Id, cancellationToken);
        if (!exists)
            return Result<DailyPlanDetailDto>.Fail(Error.NotFound("Günlük plan bulunamadı."));

        var plan = await accessScope.ApplyDailyPlanScope(db.DailyPlans.AsNoTracking())
            .Include(p => p.History)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (plan is null)
            return Result<DailyPlanDetailDto>.Fail(Error.ScopeMismatch("Bu günlük planı görüntüleme yetkiniz yok."));

        var projectInfo = await db.Projects.AsNoTracking()
            .Where(project => project.Id == plan.ProjectId)
            .Select(project => new { project.Name, project.PmUserId })
            .FirstOrDefaultAsync(cancellationToken);
        var regionInfo = await db.CrewRegions.AsNoTracking()
            .Where(region => region.Id == plan.CrewRegionId)
            .Select(region => new { region.Name, region.SiteChiefUserId })
            .FirstOrDefaultAsync(cancellationToken);
        var locationName = await db.Locations.AsNoTracking()
            .Where(location => location.Id == plan.LocationId)
            .Select(location => location.Name)
            .FirstOrDefaultAsync(cancellationToken) ?? "-";
        var workItemTypeName = await db.WorkItemTypes.AsNoTracking()
            .Where(workItemType => workItemType.Id == plan.WorkItemTypeId)
            .Select(workItemType => workItemType.Name)
            .FirstOrDefaultAsync(cancellationToken) ?? "-";

        var crewTypeName = plan.CrewTypeId is { } crewTypeId
            ? await db.CrewTypes.AsNoTracking()
                .Where(crewType => crewType.Id == crewTypeId)
                .Select(crewType => crewType.Name)
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        var userIds = plan.History.Select(h => h.ActionById)
            .Concat(new[] { plan.AssignedHoMId, regionInfo?.SiteChiefUserId, projectInfo?.PmUserId }.OfType<Guid>())
            .Distinct()
            .ToList();
        var displayNames = await identityService.GetDisplayNamesAsync(userIds, cancellationToken);

        var history = plan.History
            .OrderBy(h => h.TransitionedAt)
            .Select(h => new StatusTransitionDto(
                h.FromStatus,
                h.ToStatus,
                h.ActionById,
                h.TransitionedAt,
                displayNames.GetValueOrDefault(h.ActionById),
                h.Note))
            .ToList();

        var comments = plan.History
            .OrderBy(h => h.TransitionedAt)
            .Where(IsCommentTransition)
            .Select(h => new DailyPlanCommentDto(
                h.TransitionedAt,
                h.ActionById,
                displayNames.GetValueOrDefault(h.ActionById),
                h.FromStatus,
                h.ToStatus,
                IsRejectionTransition(h)
                    ? DailyPlanCommentKind.Rejection
                    : DailyPlanCommentKind.Progress,
                h.Note!.Trim()))
            .ToList();

        if (comments.Count == 0 && !string.IsNullOrWhiteSpace(plan.Comment))
        {
            var fallbackTransition = plan.History
                .OrderByDescending(h => h.TransitionedAt)
                .FirstOrDefault(h => h.ToStatus == Domain.Enums.WorkStatus.Submitted);
            var fallbackActorId = fallbackTransition?.ActionById ?? plan.AssignedHoMId ?? Guid.Empty;

            comments.Add(new DailyPlanCommentDto(
                fallbackTransition?.TransitionedAt ?? DateTime.UtcNow,
                fallbackActorId,
                fallbackActorId == Guid.Empty ? null : displayNames.GetValueOrDefault(fallbackActorId),
                fallbackTransition?.FromStatus ?? Domain.Enums.WorkStatus.InProgress,
                fallbackTransition?.ToStatus ?? Domain.Enums.WorkStatus.Submitted,
                DailyPlanCommentKind.Progress,
                plan.Comment.Trim()));
        }

        return new DailyPlanDetailDto(
            plan.Id,
            plan.ProjectId,
            projectInfo?.Name ?? "-",
            plan.CrewRegionId,
            regionInfo?.Name ?? "-",
            plan.LocationId,
            locationName,
            plan.WorkItemTypeId,
            workItemTypeName,
            plan.WorkDate,
            plan.PlannedById,
            plan.AssignedHoMId,
            plan.AssignedHoMId is { } assignedHoMId ? displayNames.GetValueOrDefault(assignedHoMId) : null,
            regionInfo?.SiteChiefUserId is { } siteChiefUserId ? displayNames.GetValueOrDefault(siteChiefUserId) : null,
            projectInfo?.PmUserId is { } projectManagerId ? displayNames.GetValueOrDefault(projectManagerId) : null,
            plan.CrewTypeId,
            crewTypeName,
            plan.PlannedQuantity,
            plan.PlannedManDay,
            plan.Unit,
            plan.FactQuantity,
            plan.FactManDay,
            plan.Overtime,
            plan.Comment,
            plan.Status,
            history,
            comments);
    }

    private static bool IsCommentTransition(Domain.Entities.StatusTransition transition)
    {
        if (string.IsNullOrWhiteSpace(transition.Note))
            return false;

        if (transition.ToStatus == Domain.Enums.WorkStatus.Submitted
            && transition.FromStatus == Domain.Enums.WorkStatus.InProgress)
            return true;

        if (IsRejectionTransition(transition))
            return true;

        return false;
    }

    private static bool IsRejectionTransition(Domain.Entities.StatusTransition transition)
    {
        if (transition.ToStatus == Domain.Enums.WorkStatus.Submitted
            && transition.FromStatus == Domain.Enums.WorkStatus.ApprovedBySiteChief)
            return true;

        return transition.ToStatus == Domain.Enums.WorkStatus.InProgress
               && transition.FromStatus is Domain.Enums.WorkStatus.Submitted
                   or Domain.Enums.WorkStatus.ApprovedByHoM
                   or Domain.Enums.WorkStatus.ApprovedBySiteChief;
    }
}
