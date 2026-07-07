using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.Domain.Enums;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Queries.GetDailyTrackingPlans;

public class GetDailyTrackingPlansQueryHandler
    : IRequestHandler<GetDailyTrackingPlansQuery, Result<List<DailyTrackingPlanDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly IAccessScopeService _accessScope;
    private readonly IIdentityService _identityService;

    public GetDailyTrackingPlansQueryHandler(
        IApplicationDbContext db,
        IAccessScopeService accessScope,
        IIdentityService identityService)
    {
        _db = db;
        _accessScope = accessScope;
        _identityService = identityService;
    }

    public async ValueTask<Result<List<DailyTrackingPlanDto>>> Handle(
        GetDailyTrackingPlansQuery request,
        CancellationToken cancellationToken)
    {
        var query = _accessScope.ApplyDailyPlanScope(_db.DailyPlans.AsNoTracking())
            .Where(plan => plan.WorkDate == request.WorkDate);

        query = ApplyFilters(query, request);

        var rows = await (
            from plan in query
            join project in _db.Projects.AsNoTracking() on plan.ProjectId equals project.Id
            join region in _db.CrewRegions.AsNoTracking() on plan.CrewRegionId equals region.Id
            join location in _db.Locations.AsNoTracking() on plan.LocationId equals location.Id
            join workItemType in _db.WorkItemTypes.AsNoTracking() on plan.WorkItemTypeId equals workItemType.Id
            join crewType in _db.CrewTypes.AsNoTracking() on plan.CrewTypeId equals (Guid?)crewType.Id into crewTypeJoin
            from crewType in crewTypeJoin.DefaultIfEmpty()
            orderby project.Name, region.Name, location.Name, workItemType.Name
            select new TrackingPlanRow(
                plan.Id,
                plan.ProjectId,
                project.Name,
                project.PmUserId,
                plan.CrewRegionId,
                region.Name,
                region.SiteChiefUserId,
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
                plan.Status))
            .ToListAsync(cancellationToken);

        var userIds = rows
            .SelectMany(row => new[] { row.AssignedHoMId, row.SiteChiefUserId, row.ProjectManagerUserId }.OfType<Guid>())
            .Distinct()
            .ToList();
        var displayNames = await _identityService.GetDisplayNamesAsync(userIds, cancellationToken);

        return rows.Select(row => new DailyTrackingPlanDto(
            row.Id,
            row.ProjectId,
            row.ProjectName,
            row.CrewRegionId,
            row.CrewRegionName,
            row.LocationId,
            row.LocationName,
            row.WorkItemTypeId,
            row.WorkItemTypeName,
            row.WorkDate,
            row.PlannedById,
            row.AssignedHoMId,
            row.AssignedHoMId is { } homId ? displayNames.GetValueOrDefault(homId) : null,
            row.SiteChiefUserId is { } siteChiefId ? displayNames.GetValueOrDefault(siteChiefId) : null,
            row.ProjectManagerUserId is { } pmId ? displayNames.GetValueOrDefault(pmId) : null,
            row.CrewTypeId,
            row.CrewTypeName,
            row.PlannedQuantity,
            row.PlannedManDay,
            row.Unit,
            row.FactQuantity,
            row.FactManDay,
            row.Overtime,
            row.Comment,
            row.Status)).ToList();
    }

    private IQueryable<Workplan.Domain.Entities.DailyPlan> ApplyFilters(
        IQueryable<Workplan.Domain.Entities.DailyPlan> query,
        GetDailyTrackingPlansQuery request)
    {
        if (request.ProjectIds.Count > 0)
            query = query.Where(plan => request.ProjectIds.Contains(plan.ProjectId));
        if (request.CrewRegionIds.Count > 0)
            query = query.Where(plan => request.CrewRegionIds.Contains(plan.CrewRegionId));
        if (request.LocationIds.Count > 0)
            query = query.Where(plan => request.LocationIds.Contains(plan.LocationId));
        if (request.HeadOfMasterIds.Count > 0)
            query = query.Where(plan => plan.AssignedHoMId.HasValue && request.HeadOfMasterIds.Contains(plan.AssignedHoMId.Value));
        if (request.SiteChiefIds.Count > 0)
            query = query.Where(plan => _db.CrewRegions.Any(region =>
                region.Id == plan.CrewRegionId
                && region.SiteChiefUserId.HasValue
                && request.SiteChiefIds.Contains(region.SiteChiefUserId.Value)));
        if (request.ProjectManagerIds.Count > 0)
            query = query.Where(plan => _db.Projects.Any(project =>
                project.Id == plan.ProjectId
                && project.PmUserId.HasValue
                && request.ProjectManagerIds.Contains(project.PmUserId.Value)));
        if (request.Statuses.Count > 0)
            query = query.Where(plan => request.Statuses.Contains(plan.Status));

        return query;
    }

    private sealed record TrackingPlanRow(
        Guid Id,
        Guid ProjectId,
        string ProjectName,
        Guid? ProjectManagerUserId,
        Guid CrewRegionId,
        string CrewRegionName,
        Guid? SiteChiefUserId,
        Guid LocationId,
        string LocationName,
        Guid WorkItemTypeId,
        string WorkItemTypeName,
        DateOnly WorkDate,
        Guid PlannedById,
        Guid? AssignedHoMId,
        Guid? CrewTypeId,
        string? CrewTypeName,
        decimal PlannedQuantity,
        decimal PlannedManDay,
        Workplan.Domain.ValueObjects.Unit Unit,
        decimal? FactQuantity,
        decimal? FactManDay,
        decimal? Overtime,
        string? Comment,
        WorkStatus Status);
}
