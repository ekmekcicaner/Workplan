using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Projects.Commands;

public class UpdateProjectCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateProjectCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (project is null)
            return Result.Fail(Error.NotFound("Proje bulunamadı."));

        var result = project.Update(request.Code, request.Name);
        if (result.IsFailure) return result;

        if (request.PmUserId is { } pmUserId)
        {
            var assignPm = project.AssignPm(pmUserId);
            if (assignPm.IsFailure) return assignPm;
        }

        await db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
