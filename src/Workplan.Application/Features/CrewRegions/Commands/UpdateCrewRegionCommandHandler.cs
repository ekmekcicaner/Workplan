using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.CrewRegions.Commands;

public class UpdateCrewRegionCommandHandler : IRequestHandler<UpdateCrewRegionCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public UpdateCrewRegionCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async ValueTask<Result> Handle(UpdateCrewRegionCommand request, CancellationToken cancellationToken)
    {
        var region = await _db.CrewRegions.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (region is null)
            return Result.Fail(Error.NotFound("Saha bölgesi bulunamadı."));

        var result = region.Update(request.Code, request.Name);
        if (result.IsFailure) return result;

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
