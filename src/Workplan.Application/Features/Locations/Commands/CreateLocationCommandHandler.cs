using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.Domain.Entities;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Locations.Commands;

public class CreateLocationCommandHandler : IRequestHandler<CreateLocationCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _db;

    public CreateLocationCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async ValueTask<Result<Guid>> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
    {
        var regionBelongsToProject = await _db.CrewRegions.AsNoTracking()
            .AnyAsync(r => r.Id == request.CrewRegionId && r.ProjectId == request.ProjectId, cancellationToken);
        if (!regionBelongsToProject)
            return Result<Guid>.Fail(Error.NotFound("Bölge, belirtilen projeye ait değil ya da bulunamadı."));

        if (request.ParentId is { } parentId)
        {
            var parentExists = await _db.Locations.AsNoTracking()
                .AnyAsync(l => l.Id == parentId, cancellationToken);
            if (!parentExists)
                return Result<Guid>.Fail(Error.NotFound("Üst lokasyon bulunamadı."));
        }

        var result = Location.Create(request.ProjectId, request.CrewRegionId, request.Name, request.ParentId);
        if (result.IsFailure) return Result<Guid>.Fail(result.Error);

        _db.Locations.Add(result.Value);
        await _db.SaveChangesAsync(cancellationToken);

        return result.Value.Id;
    }
}
