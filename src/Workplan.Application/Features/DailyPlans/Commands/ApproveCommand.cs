using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Commands;

// Onay: Head of Master -> Site Chief -> Project Manager sırasıyla ilerler.
// Onaylayan rol ve kullanıcı artık request body'den değil, doğrulanmış JWT claim'lerinden
// (ICurrentUserService) belirlenir; bkz. ApproverRoleMap.
public sealed record ApproveCommand(Guid DailyPlanId)
    : IRequest<Result>;
