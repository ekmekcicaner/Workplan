using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Crews.Queries.GetCrewsByLocation;

public class GetCrewsByLocationQueryHandler : IRequestHandler<GetCrewsByLocationQuery, Result<List<CrewDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly IAccessScopeService _accessScope;

    public GetCrewsByLocationQueryHandler(IApplicationDbContext db, IAccessScopeService accessScope)
    {
        _db = db;
        _accessScope = accessScope;
    }

    public async ValueTask<Result<List<CrewDto>>> Handle(
        GetCrewsByLocationQuery request, CancellationToken cancellationToken)
    {
        var crews = await _accessScope.ApplyCrewScope(_db.Crews
            .AsNoTracking()
            .Where(c => c.LocationId == request.LocationId))
            .Select(c => new CrewDto(
                c.Id,
                c.LocationId,
                c.Name,
                c.CreatedByHoMId,
                c.Members.Select(m => new CrewMemberDto(m.Id, m.WorkerType, m.PersonnelRef)).ToList()))
            .ToListAsync(cancellationToken);

        return crews;
    }
}
