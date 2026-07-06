using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.CrewRegions.Queries.GetCrewRegionById;

public class GetCrewRegionByIdQueryHandler : IRequestHandler<GetCrewRegionByIdQuery, Result<CrewRegionDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IAccessScopeService _accessScope;

    public GetCrewRegionByIdQueryHandler(IApplicationDbContext db, IAccessScopeService accessScope)
    {
        _db = db;
        _accessScope = accessScope;
    }

    public async ValueTask<Result<CrewRegionDto>> Handle(
        GetCrewRegionByIdQuery request, CancellationToken cancellationToken)
    {
        var exists = await _db.CrewRegions
            .AsNoTracking()
            .AnyAsync(r => r.Id == request.Id, cancellationToken);
        if (!exists)
            return Result<CrewRegionDto>.Fail(Error.NotFound("Saha bölgesi bulunamadı."));

        var region = await _accessScope.ApplyCrewRegionScope(_db.CrewRegions.AsNoTracking())
            .Where(r => r.Id == request.Id)
            .Select(r => new CrewRegionDto(
                r.Id, r.ProjectId, r.Code, r.Name, r.SiteChiefUserId, r.TechOfficeUserId, r.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

        return region is null
            ? Result<CrewRegionDto>.Fail(Error.ScopeMismatch("Bu saha bölgesini görüntüleme yetkiniz yok."))
            : region;
    }
}
