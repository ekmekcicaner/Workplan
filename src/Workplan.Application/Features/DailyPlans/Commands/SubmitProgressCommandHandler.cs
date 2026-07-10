using Mediator;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Commands;

public class SubmitProgressCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IAccessScopeService accessScope)
    : IRequestHandler<SubmitProgressCommand, Result>
{
    public async ValueTask<Result> Handle(SubmitProgressCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } masterId)
            return Result.Fail(Error.Unauthorized("Kimliği doğrulanmış bir kullanıcı gerekli."));

        var plan = await db.DailyPlans.FindAsync([request.DailyPlanId], cancellationToken);
        if (plan is null) return Result.Fail(Error.NotFound("Günlük plan bulunamadı."));

        if (!await accessScope.CanAccessDailyPlanAsync(request.DailyPlanId, cancellationToken))
            return Result.Fail(Error.ScopeMismatch("Bu günlük plan için gerçekleşme girme yetkiniz yok."));

        var result = plan.SubmitProgress(
            request.FactQuantity, request.FactManDay, request.Overtime, request.Comment,
            masterId);
        if (result.IsFailure) return result;

        db.StatusTransitions.Add(plan.History.Last());

        await db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
