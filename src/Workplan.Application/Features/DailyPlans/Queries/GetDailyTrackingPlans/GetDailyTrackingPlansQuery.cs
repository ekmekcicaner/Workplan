using Mediator;
using Workplan.Domain.Enums;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Queries.GetDailyTrackingPlans;

public sealed record GetDailyTrackingPlansQuery(
    DateOnly WorkDate,
    IReadOnlyCollection<Guid> ProjectIds,
    IReadOnlyCollection<Guid> CrewRegionIds,
    IReadOnlyCollection<Guid> LocationIds,
    IReadOnlyCollection<Guid> HeadOfMasterIds,
    IReadOnlyCollection<Guid> SiteChiefIds,
    IReadOnlyCollection<Guid> ProjectManagerIds,
    IReadOnlyCollection<WorkStatus> Statuses) : IRequest<Result<List<DailyTrackingPlanDto>>>;
