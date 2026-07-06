using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Locations.Queries.GetLocationById;

public class GetLocationByIdQueryHandler : IRequestHandler<GetLocationByIdQuery, Result<LocationDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IAccessScopeService _accessScope;

    public GetLocationByIdQueryHandler(IApplicationDbContext db, IAccessScopeService accessScope)
    {
        _db = db;
        _accessScope = accessScope;
    }

    public async ValueTask<Result<LocationDto>> Handle(
        GetLocationByIdQuery request, CancellationToken cancellationToken)
    {
        var exists = await _db.Locations
            .AsNoTracking()
            .AnyAsync(l => l.Id == request.Id, cancellationToken);
        if (!exists)
            return Result<LocationDto>.Fail(Error.NotFound("Lokasyon bulunamadı."));

        var location = await _accessScope.ApplyLocationScope(_db.Locations.AsNoTracking())
            .Where(l => l.Id == request.Id)
            .Select(l => new LocationDto(
                l.Id, l.ProjectId, l.CrewRegionId, l.Name, l.ParentId, l.HeadOfMasterUserId, l.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

        return location is null
            ? Result<LocationDto>.Fail(Error.ScopeMismatch("Bu lokasyonu görüntüleme yetkiniz yok."))
            : location;
    }
}
