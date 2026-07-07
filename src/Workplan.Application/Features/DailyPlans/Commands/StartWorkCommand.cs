using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Commands;

public sealed record StartWorkCommand(Guid DailyPlanId, Guid CrewTypeId) : IRequest<Result>;
