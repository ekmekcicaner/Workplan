using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Queries.GetPlansAwaitingMyApproval;

public sealed record GetPlansAwaitingMyApprovalQuery : IRequest<Result<List<DailyPlanDto>>>;
