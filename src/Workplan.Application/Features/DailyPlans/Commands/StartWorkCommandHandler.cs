using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Commands;

public class StartWorkCommandHandler : IRequestHandler<StartWorkCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public StartWorkCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async ValueTask<Result> Handle(StartWorkCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } headOfMasterUserId)
            return Result.Fail(Error.Unauthorized("Kimliği doğrulanmış bir kullanıcı gerekli."));

        var plan = await _db.DailyPlans.FindAsync([request.DailyPlanId], cancellationToken);
        if (plan is null) return Result.Fail(Error.NotFound("Günlük plan bulunamadı."));

        var crewValid = await _db.Crews.AsNoTracking()
            .AnyAsync(c => c.Id == request.CrewId
                           && c.LocationId == plan.LocationId
                           && c.CreatedByHoMId == headOfMasterUserId, cancellationToken);
        if (!crewValid)
            return Result.Fail(Error.ScopeMismatch("Ekip, lokasyon/ustabaşı ile eşleşmiyor ya da bulunamadı."));

        var result = plan.StartWork(headOfMasterUserId, request.CrewId);
        if (result.IsFailure) return result;

        _db.StatusTransitions.Add(plan.History.Last());

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
