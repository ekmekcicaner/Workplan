using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Projects.Commands;

public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public UpdateProjectCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async ValueTask<Result> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (project is null)
            return Result.Fail(Error.NotFound("Proje bulunamadı."));

        var result = project.Update(request.Code, request.Name);
        if (result.IsFailure) return result;

        if (request.PmUserId is { } pmUserId)
        {
            var assignPm = project.AssignPm(pmUserId);
            if (assignPm.IsFailure) return assignPm;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
