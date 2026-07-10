using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.WorkItemTypes.Commands;

public class UpdateWorkItemTypeCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateWorkItemTypeCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateWorkItemTypeCommand request, CancellationToken cancellationToken)
    {
        var workItemType = await db.WorkItemTypes.FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);
        if (workItemType is null)
            return Result.Fail(Error.NotFound("İş tipi bulunamadı."));

        var renameResult = workItemType.Rename(request.Name);
        if (renameResult.IsFailure) return renameResult;

        var unitResult = workItemType.SetUnit(request.Unit);
        if (unitResult.IsFailure) return unitResult;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
