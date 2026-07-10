using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.CrewRegions.Queries.GetCrewRegionsByProject;

public class GetCrewRegionsByProjectQueryHandler(IApplicationDbContext db, IAccessScopeService accessScope)
    : IRequestHandler<GetCrewRegionsByProjectQuery, Result<List<CrewRegionDto>>>
{
    public async ValueTask<Result<List<CrewRegionDto>>> Handle(
        GetCrewRegionsByProjectQuery request, CancellationToken cancellationToken)
    {
        var query = db.CrewRegions
            .AsNoTracking()
            .Where(r => r.ProjectId == request.ProjectId);

        if (!request.IncludeInactive)
            query = query.Where(r => r.IsActive);

        query = accessScope.ApplyCrewRegionScope(query);

        var regions = await query
            .Select(r => new CrewRegionDto(
                r.Id, r.ProjectId, r.Code, r.Name, r.SiteChiefUserId, r.TechOfficeUserId, r.IsActive))
            .ToListAsync(cancellationToken);

        return regions;
    }
}
