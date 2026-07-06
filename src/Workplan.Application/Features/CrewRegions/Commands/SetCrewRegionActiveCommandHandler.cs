using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.CrewRegions.Commands;

public class SetCrewRegionActiveCommandHandler : IRequestHandler<SetCrewRegionActiveCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public SetCrewRegionActiveCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async ValueTask<Result> Handle(SetCrewRegionActiveCommand request, CancellationToken cancellationToken)
    {
        var region = await _db.CrewRegions.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (region is null)
            return Result.Fail(Error.NotFound("Saha bölgesi bulunamadı."));

        if (request.IsActive) region.Activate();
        else region.Deactivate();

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
