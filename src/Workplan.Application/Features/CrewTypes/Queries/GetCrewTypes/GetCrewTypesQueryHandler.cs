using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.CrewTypes.Queries.GetCrewTypes;

public class GetCrewTypesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetCrewTypesQuery, Result<List<CrewTypeDto>>>
{
    public async ValueTask<Result<List<CrewTypeDto>>> Handle(
        GetCrewTypesQuery request,
        CancellationToken cancellationToken)
    {
        var query = db.CrewTypes.AsNoTracking();

        if (!request.IncludeInactive)
            query = query.Where(type => type.IsActive);

        var crewTypes = await query
            .OrderBy(type => !type.IsActive)
            .ThenBy(type => type.Name)
            .Select(type => new CrewTypeDto(type.Id, type.Name, type.IsActive))
            .ToListAsync(cancellationToken);

        return crewTypes;
    }
}
