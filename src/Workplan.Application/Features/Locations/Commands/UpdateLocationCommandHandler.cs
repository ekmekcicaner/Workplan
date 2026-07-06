using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Locations.Commands;

public class UpdateLocationCommandHandler : IRequestHandler<UpdateLocationCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public UpdateLocationCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async ValueTask<Result> Handle(UpdateLocationCommand request, CancellationToken cancellationToken)
    {
        var location = await _db.Locations.FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);
        if (location is null)
            return Result.Fail(Error.NotFound("Lokasyon bulunamadı."));

        var result = location.Rename(request.Name);
        if (result.IsFailure) return result;

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
