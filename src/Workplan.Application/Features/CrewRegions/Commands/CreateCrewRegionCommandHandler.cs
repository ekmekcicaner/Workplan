using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.Domain.Entities;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.CrewRegions.Commands;

public class CreateCrewRegionCommandHandler : IRequestHandler<CreateCrewRegionCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _db;

    public CreateCrewRegionCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async ValueTask<Result<Guid>> Handle(CreateCrewRegionCommand request, CancellationToken cancellationToken)
    {
        var projectExists = await _db.Projects.AsNoTracking()
            .AnyAsync(p => p.Id == request.ProjectId, cancellationToken);
        if (!projectExists)
            return Result<Guid>.Fail(Error.NotFound("Proje bulunamadı."));

        var result = CrewRegion.Create(request.ProjectId, request.Code, request.Name);
        if (result.IsFailure) return Result<Guid>.Fail(result.Error);

        if (request.SiteChiefUserId is { } siteChiefUserId)
        {
            var assignSiteChief = result.Value.AssignSiteChief(siteChiefUserId);
            if (assignSiteChief.IsFailure) return Result<Guid>.Fail(assignSiteChief.Error);
        }

        if (request.TechOfficeUserId is { } techOfficeUserId)
        {
            var assignTechOffice = result.Value.AssignTechOffice(techOfficeUserId);
            if (assignTechOffice.IsFailure) return Result<Guid>.Fail(assignTechOffice.Error);
        }

        _db.CrewRegions.Add(result.Value);
        await _db.SaveChangesAsync(cancellationToken);

        return result.Value.Id;
    }
}
