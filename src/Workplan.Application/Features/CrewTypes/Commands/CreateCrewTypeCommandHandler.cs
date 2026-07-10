using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.Domain.Entities;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.CrewTypes.Commands;

public class CreateCrewTypeCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateCrewTypeCommand, Result<Guid>>
{
    public async ValueTask<Result<Guid>> Handle(CreateCrewTypeCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        var exists = await db.CrewTypes.AsNoTracking()
            .AnyAsync(type => type.Name == name, cancellationToken);
        if (exists)
            return Result<Guid>.Fail(Error.Validation("Bu ekip tipi zaten tanımlı."));

        var result = CrewType.Create(request.Name);
        if (result.IsFailure) return Result<Guid>.Fail(result.Error);

        db.CrewTypes.Add(result.Value);
        await db.SaveChangesAsync(cancellationToken);
        return result.Value.Id;
    }
}
