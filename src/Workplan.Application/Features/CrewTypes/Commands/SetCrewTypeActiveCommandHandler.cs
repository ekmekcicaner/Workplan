using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.CrewTypes.Commands;

public class SetCrewTypeActiveCommandHandler(IApplicationDbContext db)
    : IRequestHandler<SetCrewTypeActiveCommand, Result>
{
    public async ValueTask<Result> Handle(SetCrewTypeActiveCommand request, CancellationToken cancellationToken)
    {
        var crewType = await db.CrewTypes.FirstOrDefaultAsync(type => type.Id == request.Id, cancellationToken);
        if (crewType is null)
            return Result.Fail(Error.NotFound("Ekip tipi bulunamadı."));

        if (request.IsActive) crewType.Activate();
        else crewType.Deactivate();

        await db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
