using Mediator;
using Workplan.Application.Features.DailyPlans;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Queries.GetApprovedDailyPlans;

// PM onayı tamamlanmış kayıtlar için uygulama içi okuma modeli.
public sealed record GetApprovedDailyPlansQuery : IRequest<Result<List<DailyPlanDto>>>;
