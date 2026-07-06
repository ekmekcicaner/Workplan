using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Queries.GetDailyPlanById;

public sealed record GetDailyPlanByIdQuery(Guid Id) : IRequest<Result<DailyPlanDto>>;
