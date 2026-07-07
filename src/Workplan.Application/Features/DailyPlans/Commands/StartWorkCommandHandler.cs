using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Commands;

public class StartWorkCommandHandler : IRequestHandler<StartWorkCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IAccessScopeService _accessScope;

    public StartWorkCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IAccessScopeService accessScope)
    {
        _db = db;
        _currentUser = currentUser;
        _accessScope = accessScope;
    }

    public async ValueTask<Result> Handle(StartWorkCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } headOfMasterUserId)
            return Result.Fail(Error.Unauthorized("Kimliği doğrulanmış bir kullanıcı gerekli."));

        var plan = await _db.DailyPlans.FindAsync([request.DailyPlanId], cancellationToken);
        if (plan is null) return Result.Fail(Error.NotFound("Günlük plan bulunamadı."));

        if (!await _accessScope.CanAccessDailyPlanAsync(request.DailyPlanId, cancellationToken))
            return Result.Fail(Error.ScopeMismatch("Bu günlük planı başlatma yetkiniz yok."));

        var crewTypeExists = await _db.CrewTypes.AsNoTracking()
            .AnyAsync(type => type.Id == request.CrewTypeId && type.IsActive, cancellationToken);
        if (!crewTypeExists)
            return Result.Fail(Error.Validation("Aktif bir ekip tipi seçilmelidir."));

        var result = plan.StartWork(headOfMasterUserId, request.CrewTypeId);
        if (result.IsFailure) return result;

        _db.StatusTransitions.Add(plan.History.Last());

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
