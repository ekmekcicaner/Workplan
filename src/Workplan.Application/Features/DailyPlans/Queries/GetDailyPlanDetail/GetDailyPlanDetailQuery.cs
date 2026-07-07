using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Queries.GetDailyPlanDetail;

public sealed record GetDailyPlanDetailQuery(Guid Id) : IRequest<Result<DailyPlanDetailDto>>;
