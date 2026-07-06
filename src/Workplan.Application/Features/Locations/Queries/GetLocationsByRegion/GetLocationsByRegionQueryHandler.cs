using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Locations.Queries.GetLocationsByRegion;

public class GetLocationsByRegionQueryHandler : IRequestHandler<GetLocationsByRegionQuery, Result<List<LocationDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetLocationsByRegionQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async ValueTask<Result<List<LocationDto>>> Handle(
        GetLocationsByRegionQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Locations
            .AsNoTracking()
            .Where(l => l.CrewRegionId == request.CrewRegionId);

        if (!request.IncludeInactive)
            query = query.Where(l => l.IsActive);

        var locations = await query
            .Select(l => new LocationDto(
                l.Id, l.ProjectId, l.CrewRegionId, l.Name, l.ParentId, l.HeadOfMasterUserId, l.IsActive))
            .ToListAsync(cancellationToken);

        return locations;
    }
}
