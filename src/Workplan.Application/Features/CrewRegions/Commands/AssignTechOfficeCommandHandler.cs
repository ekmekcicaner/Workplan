using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.CrewRegions.Commands;

public class AssignTechOfficeCommandHandler : IRequestHandler<AssignTechOfficeCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public AssignTechOfficeCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async ValueTask<Result> Handle(AssignTechOfficeCommand request, CancellationToken cancellationToken)
    {
        var region = await _db.CrewRegions.FirstOrDefaultAsync(r => r.Id == request.CrewRegionId, cancellationToken);
        if (region is null)
            return Result.Fail(Error.NotFound("Saha bölgesi bulunamadı."));

        var result = region.AssignTechOffice(request.TechOfficeUserId);
        if (result.IsFailure) return result;

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
