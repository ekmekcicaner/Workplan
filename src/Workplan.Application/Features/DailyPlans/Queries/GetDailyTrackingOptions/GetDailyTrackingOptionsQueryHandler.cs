using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.Domain.Enums;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Queries.GetDailyTrackingOptions;

public class GetDailyTrackingOptionsQueryHandler
    : IRequestHandler<GetDailyTrackingOptionsQuery, Result<DailyTrackingOptionsDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IAccessScopeService _accessScope;
    private readonly IIdentityService _identityService;

    public GetDailyTrackingOptionsQueryHandler(
        IApplicationDbContext db,
        IAccessScopeService accessScope,
        IIdentityService identityService)
    {
        _db = db;
        _accessScope = accessScope;
        _identityService = identityService;
    }

    public async ValueTask<Result<DailyTrackingOptionsDto>> Handle(
        GetDailyTrackingOptionsQuery request,
        CancellationToken cancellationToken)
    {
        var scopedPlans = _accessScope.ApplyDailyPlanScope(_db.DailyPlans.AsNoTracking());

        var projects = await (
            from project in _db.Projects.AsNoTracking()
            where scopedPlans.Any(plan => plan.ProjectId == project.Id)
            orderby project.Name
            select new TrackingFilterOptionDto(project.Id, project.Name))
            .ToListAsync(cancellationToken);

        var regions = await (
            from region in _db.CrewRegions.AsNoTracking()
            where scopedPlans.Any(plan => plan.CrewRegionId == region.Id)
            orderby region.Name
            select new TrackingFilterOptionDto(region.Id, region.Name))
            .ToListAsync(cancellationToken);

        var locations = await (
            from location in _db.Locations.AsNoTracking()
            where scopedPlans.Any(plan => plan.LocationId == location.Id)
            orderby location.Name
            select new TrackingFilterOptionDto(location.Id, location.Name))
            .ToListAsync(cancellationToken);

        var userIds = await (
            from plan in scopedPlans
            join project in _db.Projects.AsNoTracking() on plan.ProjectId equals project.Id
            join region in _db.CrewRegions.AsNoTracking() on plan.CrewRegionId equals region.Id
            select new
            {
                plan.AssignedHoMId,
                region.SiteChiefUserId,
                ProjectManagerUserId = project.PmUserId
            })
            .ToListAsync(cancellationToken);

        var homIds = userIds.Select(row => row.AssignedHoMId).OfType<Guid>().Distinct().ToList();
        var siteChiefIds = userIds.Select(row => row.SiteChiefUserId).OfType<Guid>().Distinct().ToList();
        var pmIds = userIds.Select(row => row.ProjectManagerUserId).OfType<Guid>().Distinct().ToList();
        var displayNames = await _identityService.GetDisplayNamesAsync(
            homIds.Concat(siteChiefIds).Concat(pmIds).Distinct().ToList(),
            cancellationToken);

        return new DailyTrackingOptionsDto(
            projects,
            regions,
            locations,
            ToUserOptions(homIds, displayNames),
            ToUserOptions(siteChiefIds, displayNames),
            ToUserOptions(pmIds, displayNames),
            Enum.GetValues<WorkStatus>());
    }

    private static List<TrackingFilterOptionDto> ToUserOptions(
        IReadOnlyCollection<Guid> userIds,
        IReadOnlyDictionary<Guid, string> displayNames) =>
        userIds
            .Select(id => new TrackingFilterOptionDto(id, displayNames.GetValueOrDefault(id) ?? id.ToString()))
            .OrderBy(option => option.Label)
            .ToList();
}
