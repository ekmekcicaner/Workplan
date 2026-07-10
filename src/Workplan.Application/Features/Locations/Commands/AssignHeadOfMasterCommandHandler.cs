using Mediator;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Locations.Commands;

public class AssignHeadOfMasterCommandHandler(IApplicationDbContext db, IAccessScopeService accessScope)
    : IRequestHandler<AssignHeadOfMasterCommand, Result>
{
    public async ValueTask<Result> Handle(AssignHeadOfMasterCommand request, CancellationToken cancellationToken)
    {
        var location = await db.Locations.FindAsync([request.LocationId], cancellationToken);
        if (location is null)
            return Result.Fail(Error.NotFound("Lokasyon bulunamadı."));

        if (!await accessScope.CanAccessLocationAsync(request.LocationId, cancellationToken))
            return Result.Fail(Error.ScopeMismatch("Bu lokasyona ustabaşı atama yetkiniz yok."));

        var result = location.AssignHeadOfMaster(request.HeadOfMasterUserId);
        if (result.IsFailure) return result;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
