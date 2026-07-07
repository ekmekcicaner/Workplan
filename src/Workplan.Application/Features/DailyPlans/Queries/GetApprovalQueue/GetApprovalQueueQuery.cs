using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Queries.GetApprovalQueue;

public sealed record GetApprovalQueueQuery : IRequest<Result<List<DailyPlanListItemDto>>>;
