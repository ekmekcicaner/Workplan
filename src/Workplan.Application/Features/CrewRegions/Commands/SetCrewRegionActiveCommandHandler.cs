using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.CrewRegions.Commands;

public class SetCrewRegionActiveCommandHandler(IApplicationDbContext db)
    : IRequestHandler<SetCrewRegionActiveCommand, Result>
{
    public async ValueTask<Result> Handle(SetCrewRegionActiveCommand request, CancellationToken cancellationToken)
    {
        var region = await db.CrewRegions.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (region is null)
            return Result.Fail(Error.NotFound("Saha bölgesi bulunamadı."));

        if (request.IsActive) region.Activate();
        else region.Deactivate();

        await db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
