using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Projects.Queries.GetProjects;

public class GetProjectsQueryHandler(IApplicationDbContext db, IAccessScopeService accessScope)
    : IRequestHandler<GetProjectsQuery, Result<List<ProjectDto>>>
{
    public async ValueTask<Result<List<ProjectDto>>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Projects.AsNoTracking();

        if (!request.IncludeInactive)
            query = query.Where(p => p.IsActive);

        query = accessScope.ApplyProjectScope(query);

        var projects = await query
            .Select(p => new ProjectDto(p.Id, p.Code, p.Name, p.PmUserId, p.IsActive))
            .ToListAsync(cancellationToken);

        return projects;
    }
}
