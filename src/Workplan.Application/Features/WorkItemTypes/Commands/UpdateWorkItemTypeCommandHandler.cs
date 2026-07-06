using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.WorkItemTypes.Commands;

public class UpdateWorkItemTypeCommandHandler : IRequestHandler<UpdateWorkItemTypeCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public UpdateWorkItemTypeCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async ValueTask<Result> Handle(UpdateWorkItemTypeCommand request, CancellationToken cancellationToken)
    {
        var workItemType = await _db.WorkItemTypes.FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);
        if (workItemType is null)
            return Result.Fail(Error.NotFound("İş tipi bulunamadı."));

        var renameResult = workItemType.Rename(request.Name);
        if (renameResult.IsFailure) return renameResult;

        var unitResult = workItemType.SetUnit(request.Unit);
        if (unitResult.IsFailure) return unitResult;

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
