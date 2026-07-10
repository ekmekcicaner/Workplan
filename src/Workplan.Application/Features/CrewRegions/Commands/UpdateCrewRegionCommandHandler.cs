using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.CrewRegions.Commands;

public class UpdateCrewRegionCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateCrewRegionCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateCrewRegionCommand request, CancellationToken cancellationToken)
    {
        var region = await db.CrewRegions.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (region is null)
            return Result.Fail(Error.NotFound("Saha bölgesi bulunamadı."));

        var result = region.Update(request.Code, request.Name);
        if (result.IsFailure) return result;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
