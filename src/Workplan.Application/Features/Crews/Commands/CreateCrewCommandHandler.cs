using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.Domain.Entities;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Crews.Commands;

public class CreateCrewCommandHandler : IRequestHandler<CreateCrewCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreateCrewCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async ValueTask<Result<Guid>> Handle(CreateCrewCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } createdByHoMId)
            return Result<Guid>.Fail(Error.Unauthorized("Kimliği doğrulanmış bir kullanıcı gerekli."));

        var locationExists = await _db.Locations.AsNoTracking()
            .AnyAsync(l => l.Id == request.LocationId, cancellationToken);
        if (!locationExists)
            return Result<Guid>.Fail(Error.NotFound("Lokasyon bulunamadı."));

        var result = Crew.Create(request.LocationId, request.Name, createdByHoMId);
        if (result.IsFailure) return Result<Guid>.Fail(result.Error);

        _db.Crews.Add(result.Value);
        await _db.SaveChangesAsync(cancellationToken);

        return result.Value.Id;
    }
}
