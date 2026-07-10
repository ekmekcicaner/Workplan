using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.WorkItemTypes.Queries.GetWorkItemTypeTree;

public class GetWorkItemTypeTreeQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetWorkItemTypeTreeQuery, Result<List<WorkItemTypeDto>>>
{
    public async ValueTask<Result<List<WorkItemTypeDto>>> Handle(
        GetWorkItemTypeTreeQuery request, CancellationToken cancellationToken)
    {
        var query = db.WorkItemTypes.AsNoTracking();

        if (!request.IncludeInactive)
            query = query.Where(w => w.IsActive);

        var flat = await query
            .Select(w => new WorkItemTypeDto(w.Id, w.Name, w.ParentId, w.Level, w.IsActive, w.Unit))
            .ToListAsync(cancellationToken);

        var byId = flat.ToDictionary(w => w.Id);
        var roots = new List<WorkItemTypeDto>();

        foreach (var node in flat)
        {
            if (node.ParentId is { } parentId && byId.TryGetValue(parentId, out var parent))
                parent.Children.Add(node);
            else
                roots.Add(node);
        }

        return roots;
    }
}
