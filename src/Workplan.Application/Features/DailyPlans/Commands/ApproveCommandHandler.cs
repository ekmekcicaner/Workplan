using Mediator;
using Workplan.Application.Interfaces;
using Workplan.Domain.Enums;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Commands;

public class ApproveCommandHandler : IRequestHandler<ApproveCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IAccessScopeService _accessScope;

    public ApproveCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IAccessScopeService accessScope)
    {
        _db = db;
        _currentUser = currentUser;
        _accessScope = accessScope;
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

        var scopeResult = await ValidateScopeAsync(plan.CrewRegionId, plan.ProjectId, approverRole, cancellationToken);
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
        CancellationToken cancellationToken)
    {
        var scopeValid = approverRole switch
        {
            WorkStatus.ApprovedBySiteChief => await _accessScope.IsSiteChiefOfCrewRegionAsync(crewRegionId, cancellationToken),
            WorkStatus.ApprovedByPM => await _accessScope.IsProjectManagerOfProjectAsync(projectId, cancellationToken),
            _ => false
        };

        if (!scopeValid)
            return Result<Guid>.Fail(Error.ScopeMismatch("Bu günlük plan için onay yetkiniz yok."));

        return approverRole == WorkStatus.ApprovedBySiteChief ? crewRegionId : projectId;
    }
}
