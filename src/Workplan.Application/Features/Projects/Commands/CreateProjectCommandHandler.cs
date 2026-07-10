using Mediator;
using Workplan.Application.Interfaces;
using Workplan.Domain.Entities;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Projects.Commands;

public class CreateProjectCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateProjectCommand, Result<Guid>>
{
    public async ValueTask<Result<Guid>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var result = Project.Create(request.Code, request.Name, request.PmUserId);
        if (result.IsFailure) return Result<Guid>.Fail(result.Error);

        db.Projects.Add(result.Value);
        await db.SaveChangesAsync(cancellationToken);

        return result.Value.Id;
    }
}
