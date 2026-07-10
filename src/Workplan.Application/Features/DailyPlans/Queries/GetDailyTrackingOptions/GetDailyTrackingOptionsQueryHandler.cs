using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.Domain.Enums;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Queries.GetDailyTrackingOptions;

public class GetDailyTrackingOptionsQueryHandler(
    IApplicationDbContext db,
    IAccessScopeService accessScope,
    IIdentityService identityService)
    : IRequestHandler<GetDailyTrackingOptionsQuery, Result<DailyTrackingOptionsDto>>
{
    public async ValueTask<Result<DailyTrackingOptionsDto>> Handle(
        GetDailyTrackingOptionsQuery request,
        CancellationToken cancellationToken)
    {
        var scopedPlans = accessScope.ApplyDailyPlanScope(db.DailyPlans.AsNoTracking());

        var projects = await (
            from project in db.Projects.AsNoTracking()
            where scopedPlans.Any(plan => plan.ProjectId == project.Id)
            orderby project.Name
            select new TrackingFilterOptionDto(project.Id, project.Name))
            .ToListAsync(cancellationToken);

        var regions = await (
            from region in db.CrewRegions.AsNoTracking()
            where scopedPlans.Any(plan => plan.CrewRegionId == region.Id)
            orderby region.Name
            select new TrackingFilterOptionDto(region.Id, region.Name))
            .ToListAsync(cancellationToken);

        var locations = await (
            from location in db.Locations.AsNoTracking()
            where scopedPlans.Any(plan => plan.LocationId == location.Id)
            orderby location.Name
            select new TrackingFilterOptionDto(location.Id, location.Name))
            .ToListAsync(cancellationToken);

        var userIds = await (
            from plan in scopedPlans
            join project in db.Projects.AsNoTracking() on plan.ProjectId equals project.Id
            join region in db.CrewRegions.AsNoTracking() on plan.CrewRegionId equals region.Id
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
        var displayNames = await identityService.GetDisplayNamesAsync(
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
