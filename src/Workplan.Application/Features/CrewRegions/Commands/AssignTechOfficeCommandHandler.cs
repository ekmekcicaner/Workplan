using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.CrewRegions.Commands;

public class AssignTechOfficeCommandHandler(IApplicationDbContext db, IAccessScopeService accessScope)
    : IRequestHandler<AssignTechOfficeCommand, Result>
{
    public async ValueTask<Result> Handle(AssignTechOfficeCommand request, CancellationToken cancellationToken)
    {
        var region = await db.CrewRegions.FirstOrDefaultAsync(r => r.Id == request.CrewRegionId, cancellationToken);
        if (region is null)
            return Result.Fail(Error.NotFound("Saha bölgesi bulunamadı."));

        if (!await accessScope.CanAccessCrewRegionAsync(request.CrewRegionId, cancellationToken))
            return Result.Fail(Error.ScopeMismatch("Bu saha bölgesine teknik ofis atama yetkiniz yok."));

        var result = region.AssignTechOffice(request.TechOfficeUserId);
        if (result.IsFailure) return result;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
