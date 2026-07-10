using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.Domain.Entities;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.WorkItemTypes.Commands;

public class CreateWorkItemTypeCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateWorkItemTypeCommand, Result<Guid>>
{
    public async ValueTask<Result<Guid>> Handle(CreateWorkItemTypeCommand request, CancellationToken cancellationToken)
    {
        int? parentLevel = null;
        if (request.ParentId is { } parentId)
        {
            var parent = await db.WorkItemTypes.AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == parentId, cancellationToken);
            if (parent is null)
                return Result<Guid>.Fail(Error.NotFound("Üst iş tipi bulunamadı."));

            parentLevel = parent.Level;
        }

        var result = WorkItemType.Create(request.Name, request.ParentId, parentLevel, request.Unit);
        if (result.IsFailure) return Result<Guid>.Fail(result.Error);

        db.WorkItemTypes.Add(result.Value);
        await db.SaveChangesAsync(cancellationToken);

        return result.Value.Id;
    }
}
