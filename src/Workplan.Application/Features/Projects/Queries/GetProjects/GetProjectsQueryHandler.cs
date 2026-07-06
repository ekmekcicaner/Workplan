using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;
using Roles = Workplan.SharedKernel.Auth.Roles;

namespace Workplan.Application.Features.Projects.Queries.GetProjects;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, Result<List<ProjectDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetProjectsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async ValueTask<Result<List<ProjectDto>>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Projects.AsNoTracking();

        if (!request.IncludeInactive)
            query = query.Where(p => p.IsActive);

        // PM'ler yalnızca kendilerine atanan projeleri görebilir; diğer roller tüm projeleri görür.
        if (_currentUser.Roles.Contains(Roles.ProjectManager) && _currentUser.Roles.Count == 1)
            query = query.Where(p => p.PmUserId == _currentUser.UserId);

        var projects = await query
            .Select(p => new ProjectDto(p.Id, p.Code, p.Name, p.PmUserId, p.IsActive))
            .ToListAsync(cancellationToken);

        return projects;
    }
}
