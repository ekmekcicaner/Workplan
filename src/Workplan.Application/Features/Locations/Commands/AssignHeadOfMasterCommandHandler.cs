using Mediator;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Locations.Commands;

public class AssignHeadOfMasterCommandHandler : IRequestHandler<AssignHeadOfMasterCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public AssignHeadOfMasterCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async ValueTask<Result> Handle(AssignHeadOfMasterCommand request, CancellationToken cancellationToken)
    {
        var location = await _db.Locations.FindAsync([request.LocationId], cancellationToken);
        if (location is null)
            return Result.Fail(Error.NotFound("Lokasyon bulunamadı."));

        var result = location.AssignHeadOfMaster(request.HeadOfMasterUserId);
        if (result.IsFailure) return result;

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
