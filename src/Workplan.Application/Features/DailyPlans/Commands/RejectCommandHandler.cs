using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.Domain.Enums;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Commands;

public class RejectCommandHandler : IRequestHandler<RejectCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public RejectCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async ValueTask<Result> Handle(RejectCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } approverUserId)
            return Result.Fail(Error.Unauthorized("Kimliği doğrulanmış bir kullanıcı gerekli."));

        var roleResult = ApproverRoleMap.Resolve(_currentUser.Roles);
        if (roleResult.IsFailure) return Result.Fail(roleResult.Error);

        var plan = await _db.DailyPlans.FindAsync([request.DailyPlanId], cancellationToken);
        if (plan is null) return Result.Fail(Error.NotFound("Günlük plan bulunamadı."));

        var scopeResult = await ValidateScopeAsync(plan.CrewRegionId, plan.ProjectId, roleResult.Value, approverUserId,
            cancellationToken);
        if (scopeResult.IsFailure) return scopeResult;

        var result = plan.Reject(roleResult.Value, approverUserId, request.Reason);
        if (result.IsFailure) return result;

        _db.StatusTransitions.Add(plan.History.Last());

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    private async Task<Result> ValidateScopeAsync(
        Guid crewRegionId,
        Guid projectId,
        WorkStatus approverRole,
        Guid approverUserId,
        CancellationToken cancellationToken)
    {
        var scopeValid = approverRole switch
        {
            WorkStatus.ApprovedBySiteChief => await _db.CrewRegions.AsNoTracking()
                .AnyAsync(r => r.Id == crewRegionId && r.SiteChiefUserId == approverUserId, cancellationToken),
            WorkStatus.ApprovedByPM => await _db.Projects.AsNoTracking()
                .AnyAsync(p => p.Id == projectId && p.PmUserId == approverUserId, cancellationToken),
            _ => false
        };

        return scopeValid
            ? Result.Ok()
            : Result.Fail(Error.ScopeMismatch("Bu günlük plan için red yetkiniz yok."));
    }
}
