using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.WorkItemTypes.Commands;

public class SetWorkItemTypeActiveCommandHandler : IRequestHandler<SetWorkItemTypeActiveCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public SetWorkItemTypeActiveCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async ValueTask<Result> Handle(SetWorkItemTypeActiveCommand request, CancellationToken cancellationToken)
    {
        var workItemType = await _db.WorkItemTypes.FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);
        if (workItemType is null)
            return Result.Fail(Error.NotFound("İş tipi bulunamadı."));

        if (request.IsActive) workItemType.Activate();
        else workItemType.Deactivate();

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
