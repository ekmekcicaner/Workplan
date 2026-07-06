using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.CrewRegions.Queries.GetCrewRegionById;

public class GetCrewRegionByIdQueryHandler : IRequestHandler<GetCrewRegionByIdQuery, Result<CrewRegionDto>>
{
    private readonly IApplicationDbContext _db;

    public GetCrewRegionByIdQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async ValueTask<Result<CrewRegionDto>> Handle(
        GetCrewRegionByIdQuery request, CancellationToken cancellationToken)
    {
        var region = await _db.CrewRegions
            .AsNoTracking()
            .Where(r => r.Id == request.Id)
            .Select(r => new CrewRegionDto(
                r.Id, r.ProjectId, r.Code, r.Name, r.SiteChiefUserId, r.TechOfficeUserId, r.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

        return region is null
            ? Result<CrewRegionDto>.Fail(Error.NotFound("Saha bölgesi bulunamadı."))
            : region;
    }
}
