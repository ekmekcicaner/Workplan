using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.Domain.Entities;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.CrewTypes.Commands;

public class CreateCrewTypeCommandHandler : IRequestHandler<CreateCrewTypeCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _db;

    public CreateCrewTypeCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async ValueTask<Result<Guid>> Handle(CreateCrewTypeCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        var exists = await _db.CrewTypes.AsNoTracking()
            .AnyAsync(type => type.Name == name, cancellationToken);
        if (exists)
            return Result<Guid>.Fail(Error.Validation("Bu ekip tipi zaten tanımlı."));

        var result = CrewType.Create(request.Name);
        if (result.IsFailure) return Result<Guid>.Fail(result.Error);

        _db.CrewTypes.Add(result.Value);
        await _db.SaveChangesAsync(cancellationToken);
        return result.Value.Id;
    }
}
