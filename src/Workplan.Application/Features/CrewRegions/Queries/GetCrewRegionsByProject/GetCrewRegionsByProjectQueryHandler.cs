using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.CrewRegions.Queries.GetCrewRegionsByProject;

public class GetCrewRegionsByProjectQueryHandler
    : IRequestHandler<GetCrewRegionsByProjectQuery, Result<List<CrewRegionDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetCrewRegionsByProjectQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async ValueTask<Result<List<CrewRegionDto>>> Handle(
        GetCrewRegionsByProjectQuery request, CancellationToken cancellationToken)
    {
        var query = _db.CrewRegions
            .AsNoTracking()
            .Where(r => r.ProjectId == request.ProjectId);

        if (!request.IncludeInactive)
            query = query.Where(r => r.IsActive);

        var regions = await query
            .Select(r => new CrewRegionDto(
                r.Id, r.ProjectId, r.Code, r.Name, r.SiteChiefUserId, r.TechOfficeUserId, r.IsActive))
            .ToListAsync(cancellationToken);

        return regions;
    }
}
