using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Commands;

public class StartWorkCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IAccessScopeService accessScope)
    : IRequestHandler<StartWorkCommand, Result>
{
    public async ValueTask<Result> Handle(StartWorkCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } headOfMasterUserId)
            return Result.Fail(Error.Unauthorized("Kimliği doğrulanmış bir kullanıcı gerekli."));

        var plan = await db.DailyPlans.FindAsync([request.DailyPlanId], cancellationToken);
        if (plan is null) return Result.Fail(Error.NotFound("Günlük plan bulunamadı."));

        if (!await accessScope.CanAccessDailyPlanAsync(request.DailyPlanId, cancellationToken))
            return Result.Fail(Error.ScopeMismatch("Bu günlük planı başlatma yetkiniz yok."));

        var crewTypeExists = await db.CrewTypes.AsNoTracking()
            .AnyAsync(type => type.Id == request.CrewTypeId && type.IsActive, cancellationToken);
        if (!crewTypeExists)
            return Result.Fail(Error.Validation("Aktif bir ekip tipi seçilmelidir."));

        var result = plan.StartWork(headOfMasterUserId, request.CrewTypeId);
        if (result.IsFailure) return result;

        db.StatusTransitions.Add(plan.History.Last());

        await db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
