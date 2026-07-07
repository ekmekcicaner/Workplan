using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Queries.GetMyWorkDailyPlans;

public sealed record GetMyWorkDailyPlansQuery : IRequest<Result<List<DailyPlanListItemDto>>>;
