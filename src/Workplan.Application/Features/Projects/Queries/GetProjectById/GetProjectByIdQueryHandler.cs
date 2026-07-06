using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Projects.Queries.GetProjectById;

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, Result<ProjectDto>>
{
    private readonly IApplicationDbContext _db;

    public GetProjectByIdQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async ValueTask<Result<ProjectDto>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await _db.Projects
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(p => new ProjectDto(p.Id, p.Code, p.Name, p.PmUserId, p.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

        return project is null
            ? Result<ProjectDto>.Fail(Error.NotFound("Proje bulunamadı."))
            : project;
    }
}
