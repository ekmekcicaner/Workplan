using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Crews.Commands;

public class AddCrewMemberCommandHandler : IRequestHandler<AddCrewMemberCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _db;

    public AddCrewMemberCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async ValueTask<Result<Guid>> Handle(AddCrewMemberCommand request, CancellationToken cancellationToken)
    {
        var crew = await _db.Crews
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == request.CrewId, cancellationToken);
        if (crew is null)
            return Result<Guid>.Fail(Error.NotFound("Ekip bulunamadı."));

        var result = crew.AddMember(request.WorkerType, request.PersonnelRef);
        if (result.IsFailure) return Result<Guid>.Fail(result.Error);

        // Crew zaten tracked (Unchanged) olduğu için, koleksiyona yeni eklenen CrewMember EF
        // tarafından "Added" değil "Modified" sanılabiliyor (client-generated Guid key + disconnected
        // graph). Açıkça Added olarak işaretlemek için ayrıca ekliyoruz.
        _db.CrewMembers.Add(result.Value);

        await _db.SaveChangesAsync(cancellationToken);
        return result.Value.Id;
    }
}
