using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.CrewRegions.Commands;

public class AssignSiteChiefCommandHandler : IRequestHandler<AssignSiteChiefCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly IAccessScopeService _accessScope;

    public AssignSiteChiefCommandHandler(IApplicationDbContext db, IAccessScopeService accessScope)
    {
        _db = db;
        _accessScope = accessScope;
    }

    public async ValueTask<Result> Handle(AssignSiteChiefCommand request, CancellationToken cancellationToken)
    {
        var region = await _db.CrewRegions.FirstOrDefaultAsync(r => r.Id == request.CrewRegionId, cancellationToken);
        if (region is null)
            return Result.Fail(Error.NotFound("Saha bölgesi bulunamadı."));

        if (!await _accessScope.CanAccessCrewRegionAsync(request.CrewRegionId, cancellationToken))
            return Result.Fail(Error.ScopeMismatch("Bu saha bölgesine şantiye şefi atama yetkiniz yok."));

        var result = region.AssignSiteChief(request.SiteChiefUserId);
        if (result.IsFailure) return result;

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
