using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Locations.Queries.GetLocationsByRegion;

public class GetLocationsByRegionQueryHandler(IApplicationDbContext db, IAccessScopeService accessScope)
    : IRequestHandler<GetLocationsByRegionQuery, Result<List<LocationDto>>>
{
    public async ValueTask<Result<List<LocationDto>>> Handle(
        GetLocationsByRegionQuery request, CancellationToken cancellationToken)
    {
        var query = db.Locations
            .AsNoTracking()
            .Where(l => l.CrewRegionId == request.CrewRegionId);

        if (!request.IncludeInactive)
            query = query.Where(l => l.IsActive);

        query = accessScope.ApplyLocationScope(query);

        var locations = await query
            .Select(l => new LocationDto(
                l.Id, l.ProjectId, l.CrewRegionId, l.Name, l.ParentId, l.HeadOfMasterUserId, l.IsActive))
            .ToListAsync(cancellationToken);

        return locations;
    }
}
