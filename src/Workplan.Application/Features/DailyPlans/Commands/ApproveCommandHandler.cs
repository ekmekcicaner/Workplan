using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.Domain.Enums;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Commands;

public class ApproveCommandHandler : IRequestHandler<ApproveCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ApproveCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async ValueTask<Result> Handle(ApproveCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } approverUserId)
            return Result.Fail(Error.Unauthorized("Kimliği doğrulanmış bir kullanıcı gerekli."));

        var roleResult = ApproverRoleMap.Resolve(_currentUser.Roles);
        if (roleResult.IsFailure) return Result.Fail(roleResult.Error);
        var approverRole = roleResult.Value;

        var plan = await _db.DailyPlans.FindAsync([request.DailyPlanId], cancellationToken);
        if (plan is null) return Result.Fail(Error.NotFound("Günlük plan bulunamadı."));

        var scopeResult = await ValidateScopeAsync(plan.CrewRegionId, plan.ProjectId, approverRole, approverUserId,
            cancellationToken);
        if (scopeResult.IsFailure) return scopeResult;

        var result = plan.Approve(approverRole, scopeResult.Value, approverUserId);
        if (result.IsFailure) return result;

        _db.StatusTransitions.Add(plan.History.Last());

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    private async Task<Result<Guid>> ValidateScopeAsync(
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

        if (!scopeValid)
            return Result<Guid>.Fail(Error.ScopeMismatch("Bu günlük plan için onay yetkiniz yok."));

        return approverRole == WorkStatus.ApprovedBySiteChief ? crewRegionId : projectId;
    }
}
