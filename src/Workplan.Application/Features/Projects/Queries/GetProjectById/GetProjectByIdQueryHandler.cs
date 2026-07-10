using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Projects.Queries.GetProjectById;

public class GetProjectByIdQueryHandler(IApplicationDbContext db, IAccessScopeService accessScope)
    : IRequestHandler<GetProjectByIdQuery, Result<ProjectDto>>
{
    public async ValueTask<Result<ProjectDto>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var exists = await db.Projects
            .AsNoTracking()
            .AnyAsync(p => p.Id == request.Id, cancellationToken);
        if (!exists)
            return Result<ProjectDto>.Fail(Error.NotFound("Proje bulunamadı."));

        var project = await accessScope.ApplyProjectScope(db.Projects.AsNoTracking())
            .Where(p => p.Id == request.Id)
            .Select(p => new ProjectDto(p.Id, p.Code, p.Name, p.PmUserId, p.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

        return project is null
            ? Result<ProjectDto>.Fail(Error.ScopeMismatch("Bu projeyi görüntüleme yetkiniz yok."))
            : project;
    }
}
