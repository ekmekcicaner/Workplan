using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.Domain.Entities;
using Workplan.Domain.Enums;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Commands;

public class RejectCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IAccessScopeService accessScope)
    : IRequestHandler<RejectCommand, Result>
{
    public async ValueTask<Result> Handle(RejectCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } approverUserId)
            return Result.Fail(Error.Unauthorized("Kimliği doğrulanmış bir kullanıcı gerekli."));

        var roleResult = ApproverRoleMap.Resolve(currentUser.Roles);
        if (roleResult.IsFailure) return Result.Fail(roleResult.Error);

        var plan = await db.DailyPlans.FindAsync([request.DailyPlanId], cancellationToken);
        if (plan is null) return Result.Fail(Error.NotFound("Günlük plan bulunamadı."));

        var scopeResult = await ValidateScopeAsync(plan.CrewRegionId, plan.ProjectId, roleResult.Value, cancellationToken);
        if (scopeResult.IsFailure) return scopeResult;

        var statusBeforeReject = plan.Status;
        var result = plan.Reject(roleResult.Value, approverUserId, request.Reason);
        if (result.IsFailure) return result;

        var transition = plan.History.Last();
        var recipientResult = await ResolveRejectRecipientAsync(plan, roleResult.Value, statusBeforeReject, cancellationToken);
        if (recipientResult.IsFailure) return Result.Fail(recipientResult.Error);

        var notification = Notification.CreateDailyPlanRejected(
            recipientResult.Value,
            plan.Id,
            plan.WorkDate,
            RejecterLabel(roleResult.Value),
            transition.Note ?? request.Reason);
        if (notification.IsFailure) return Result.Fail(notification.Error);

        db.StatusTransitions.Add(transition);
        db.Notifications.Add(notification.Value);

        await db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    private async Task<Result> ValidateScopeAsync(
        Guid crewRegionId,
        Guid projectId,
        WorkStatus approverRole,
        CancellationToken cancellationToken)
    {
        var scopeValid = approverRole switch
        {
            WorkStatus.ApprovedBySiteChief => await accessScope.IsSiteChiefOfCrewRegionAsync(crewRegionId, cancellationToken),
            WorkStatus.ApprovedByPM => await accessScope.IsProjectManagerOfProjectAsync(projectId, cancellationToken),
            _ => false
        };

        return scopeValid
            ? Result.Ok()
            : Result.Fail(Error.ScopeMismatch("Bu günlük plan için red yetkiniz yok."));
    }

    private async Task<Result<Guid>> ResolveRejectRecipientAsync(
        Domain.Entities.DailyPlan plan,
        WorkStatus rejecterRole,
        WorkStatus statusBeforeReject,
        CancellationToken cancellationToken)
    {
        if (rejecterRole == WorkStatus.ApprovedBySiteChief
            && statusBeforeReject == WorkStatus.Submitted)
        {
            return plan.AssignedHoMId is { } assignedHoMId
                ? Result<Guid>.Ok(assignedHoMId)
                : Result<Guid>.Fail(Error.Validation("Atanmış ustabaşı bulunamadı."));
        }

        if (rejecterRole == WorkStatus.ApprovedByPM && statusBeforeReject == WorkStatus.ApprovedBySiteChief)
        {
            var siteChiefUserId = await db.CrewRegions
                .AsNoTracking()
                .Where(region => region.Id == plan.CrewRegionId)
                .Select(region => region.SiteChiefUserId)
                .FirstOrDefaultAsync(cancellationToken);

            return siteChiefUserId is { } userId
                ? Result<Guid>.Ok(userId)
                : Result<Guid>.Fail(Error.Validation("Atanmış şantiye şefi bulunamadı."));
        }

        return Result<Guid>.Fail(Error.Validation("Red bildirimi için önceki sorumlu belirlenemedi."));
    }

    private static string RejecterLabel(WorkStatus rejecterRole) => rejecterRole switch
    {
        WorkStatus.ApprovedBySiteChief => "Şantiye Şefi",
        WorkStatus.ApprovedByPM => "Project Manager",
        _ => "Onaycı"
    };
}
