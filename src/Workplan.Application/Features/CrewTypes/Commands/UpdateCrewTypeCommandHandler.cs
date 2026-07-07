using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.CrewTypes.Commands;

public class UpdateCrewTypeCommandHandler : IRequestHandler<UpdateCrewTypeCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public UpdateCrewTypeCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async ValueTask<Result> Handle(UpdateCrewTypeCommand request, CancellationToken cancellationToken)
    {
        var crewType = await _db.CrewTypes.FirstOrDefaultAsync(type => type.Id == request.Id, cancellationToken);
        if (crewType is null)
            return Result.Fail(Error.NotFound("Ekip tipi bulunamadı."));

        var name = request.Name.Trim();
        var duplicate = await _db.CrewTypes.AsNoTracking()
            .AnyAsync(type => type.Id != request.Id && type.Name == name, cancellationToken);
        if (duplicate)
            return Result.Fail(Error.Validation("Bu ekip tipi zaten tanımlı."));

        var result = crewType.Rename(request.Name);
        if (result.IsFailure) return result;

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
