using Mediator;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Commands;

public class SubmitProgressCommandHandler : IRequestHandler<SubmitProgressCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IAccessScopeService _accessScope;

    public SubmitProgressCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IAccessScopeService accessScope)
    {
        _db = db;
        _currentUser = currentUser;
        _accessScope = accessScope;
    }

    public async ValueTask<Result> Handle(SubmitProgressCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } masterId)
            return Result.Fail(Error.Unauthorized("Kimliği doğrulanmış bir kullanıcı gerekli."));

        var plan = await _db.DailyPlans.FindAsync([request.DailyPlanId], cancellationToken);
        if (plan is null) return Result.Fail(Error.NotFound("Günlük plan bulunamadı."));

        if (!await _accessScope.CanAccessDailyPlanAsync(request.DailyPlanId, cancellationToken))
            return Result.Fail(Error.ScopeMismatch("Bu günlük plan için gerçekleşme girme yetkiniz yok."));

        var result = plan.SubmitProgress(
            request.FactQuantity, request.FactManDay, request.Overtime, request.Comment,
            masterId);
        if (result.IsFailure) return result;

        _db.StatusTransitions.Add(plan.History.Last());

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
