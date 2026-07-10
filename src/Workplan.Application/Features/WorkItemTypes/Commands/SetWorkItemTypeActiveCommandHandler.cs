using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.WorkItemTypes.Commands;

public class SetWorkItemTypeActiveCommandHandler(IApplicationDbContext db)
    : IRequestHandler<SetWorkItemTypeActiveCommand, Result>
{
    public async ValueTask<Result> Handle(SetWorkItemTypeActiveCommand request, CancellationToken cancellationToken)
    {
        var workItemType = await db.WorkItemTypes.FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);
        if (workItemType is null)
            return Result.Fail(Error.NotFound("İş tipi bulunamadı."));

        if (request.IsActive) workItemType.Activate();
        else workItemType.Deactivate();

        await db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
