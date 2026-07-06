using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Projects.Commands;

public class SetProjectActiveCommandHandler : IRequestHandler<SetProjectActiveCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public SetProjectActiveCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async ValueTask<Result> Handle(SetProjectActiveCommand request, CancellationToken cancellationToken)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (project is null)
            return Result.Fail(Error.NotFound("Proje bulunamadı."));

        if (request.IsActive) project.Activate();
        else project.Deactivate();

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
