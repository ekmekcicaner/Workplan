using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Locations.Commands;

public class SetLocationActiveCommandHandler : IRequestHandler<SetLocationActiveCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public SetLocationActiveCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async ValueTask<Result> Handle(SetLocationActiveCommand request, CancellationToken cancellationToken)
    {
        var location = await _db.Locations.FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);
        if (location is null)
            return Result.Fail(Error.NotFound("Lokasyon bulunamadı."));

        if (request.IsActive) location.Activate();
        else location.Deactivate();

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
