using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Queries.GetDailyTrackingOptions;

public sealed record GetDailyTrackingOptionsQuery : IRequest<Result<DailyTrackingOptionsDto>>;
