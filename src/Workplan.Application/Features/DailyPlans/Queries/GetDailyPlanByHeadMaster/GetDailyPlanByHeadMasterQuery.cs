using Mediator;
using Workplan.Application.Features.DailyPlans;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Queries.GetDailyPlanByHeadMaster;

// T0: Head of Master'ın, kendi lokasyonuna (KKK) atanmış günlük planları görüntülemesi.
public sealed record GetDailyPlanByHeadMasterQuery(Guid HeadOfMasterUserId) : IRequest<Result<List<DailyPlanDto>>>;
