using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Locations.Commands;

public class SetLocationActiveCommandHandler(IApplicationDbContext db)
    : IRequestHandler<SetLocationActiveCommand, Result>
{
    public async ValueTask<Result> Handle(SetLocationActiveCommand request, CancellationToken cancellationToken)
    {
        var location = await db.Locations.FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);
        if (location is null)
            return Result.Fail(Error.NotFound("Lokasyon bulunamadı."));

        if (request.IsActive) location.Activate();
        else location.Deactivate();

        await db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
