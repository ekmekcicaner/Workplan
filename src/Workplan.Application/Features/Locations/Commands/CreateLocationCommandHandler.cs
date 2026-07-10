using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.Domain.Entities;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Locations.Commands;

public class CreateLocationCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateLocationCommand, Result<Guid>>
{
    public async ValueTask<Result<Guid>> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
    {
        var regionBelongsToProject = await db.CrewRegions.AsNoTracking()
            .AnyAsync(r => r.Id == request.CrewRegionId && r.ProjectId == request.ProjectId, cancellationToken);
        if (!regionBelongsToProject)
            return Result<Guid>.Fail(Error.NotFound("Bölge, belirtilen projeye ait değil ya da bulunamadı."));

        if (request.ParentId is { } parentId)
        {
            var parentExists = await db.Locations.AsNoTracking()
                .AnyAsync(l => l.Id == parentId, cancellationToken);
            if (!parentExists)
                return Result<Guid>.Fail(Error.NotFound("Üst lokasyon bulunamadı."));
        }

        var result = Location.Create(request.ProjectId, request.CrewRegionId, request.Name, request.ParentId);
        if (result.IsFailure) return Result<Guid>.Fail(result.Error);

        db.Locations.Add(result.Value);
        await db.SaveChangesAsync(cancellationToken);

        return result.Value.Id;
    }
}
