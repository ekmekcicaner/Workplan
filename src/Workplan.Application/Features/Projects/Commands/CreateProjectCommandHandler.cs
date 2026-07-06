using Mediator;
using Workplan.Application.Interfaces;
using Workplan.Domain.Entities;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Projects.Commands;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _db;

    public CreateProjectCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async ValueTask<Result<Guid>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var result = Project.Create(request.Code, request.Name, request.PmUserId);
        if (result.IsFailure) return Result<Guid>.Fail(result.Error);

        _db.Projects.Add(result.Value);
        await _db.SaveChangesAsync(cancellationToken);

        return result.Value.Id;
    }
}
