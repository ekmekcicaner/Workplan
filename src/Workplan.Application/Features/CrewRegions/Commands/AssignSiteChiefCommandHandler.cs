using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.CrewRegions.Commands;

public class AssignSiteChiefCommandHandler(IApplicationDbContext db, IAccessScopeService accessScope)
    : IRequestHandler<AssignSiteChiefCommand, Result>
{
    public async ValueTask<Result> Handle(AssignSiteChiefCommand request, CancellationToken cancellationToken)
    {
        var region = await db.CrewRegions.FirstOrDefaultAsync(r => r.Id == request.CrewRegionId, cancellationToken);
        if (region is null)
            return Result.Fail(Error.NotFound("Saha bölgesi bulunamadı."));

        if (!await accessScope.CanAccessCrewRegionAsync(request.CrewRegionId, cancellationToken))
            return Result.Fail(Error.ScopeMismatch("Bu saha bölgesine şantiye şefi atama yetkiniz yok."));

        var result = region.AssignSiteChief(request.SiteChiefUserId);
        if (result.IsFailure) return result;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
