using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Locations.Queries.GetLocationById;

public class GetLocationByIdQueryHandler : IRequestHandler<GetLocationByIdQuery, Result<LocationDto>>
{
    private readonly IApplicationDbContext _db;

    public GetLocationByIdQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async ValueTask<Result<LocationDto>> Handle(
        GetLocationByIdQuery request, CancellationToken cancellationToken)
    {
        var location = await _db.Locations
            .AsNoTracking()
            .Where(l => l.Id == request.Id)
            .Select(l => new LocationDto(
                l.Id, l.ProjectId, l.CrewRegionId, l.Name, l.ParentId, l.HeadOfMasterUserId, l.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

        return location is null
            ? Result<LocationDto>.Fail(Error.NotFound("Lokasyon bulunamadı."))
            : location;
    }
}
