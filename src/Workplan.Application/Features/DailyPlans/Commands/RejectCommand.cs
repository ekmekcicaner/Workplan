using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Commands;

// Reddeden rol ve kullanıcı artık request body'den değil, doğrulanmış JWT claim'lerinden
// (ICurrentUserService) belirlenir; bkz. ApproverRoleMap.
public sealed record RejectCommand(Guid DailyPlanId, string Reason)
    : IRequest<Result>;
