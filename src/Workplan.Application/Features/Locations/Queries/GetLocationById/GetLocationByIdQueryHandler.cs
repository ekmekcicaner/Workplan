using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Locations.Queries.GetLocationById;

public class GetLocationByIdQueryHandler(IApplicationDbContext db, IAccessScopeService accessScope)
    : IRequestHandler<GetLocationByIdQuery, Result<LocationDto>>
{
    public async ValueTask<Result<LocationDto>> Handle(
        GetLocationByIdQuery request, CancellationToken cancellationToken)
    {
        var exists = await db.Locations
            .AsNoTracking()
            .AnyAsync(l => l.Id == request.Id, cancellationToken);
        if (!exists)
            return Result<LocationDto>.Fail(Error.NotFound("Lokasyon bulunamadı."));

        var location = await accessScope.ApplyLocationScope(db.Locations.AsNoTracking())
            .Where(l => l.Id == request.Id)
            .Select(l => new LocationDto(
                l.Id, l.ProjectId, l.CrewRegionId, l.Name, l.ParentId, l.HeadOfMasterUserId, l.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

        return location is null
            ? Result<LocationDto>.Fail(Error.ScopeMismatch("Bu lokasyonu görüntüleme yetkiniz yok."))
            : location;
    }
}
