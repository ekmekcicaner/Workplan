using Mediator;
using Workplan.Application.Features.DailyPlans;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Queries.GetApprovedDailyPlans;

// Raporlama seam'i: Power BI, Approve akışının tamamlandığı (PM onayı) kayıtları buradan çeker.
public sealed record GetApprovedDailyPlansQuery : IRequest<Result<List<DailyPlanDto>>>;
