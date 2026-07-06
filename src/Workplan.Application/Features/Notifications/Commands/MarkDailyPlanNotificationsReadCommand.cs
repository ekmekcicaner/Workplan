using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Notifications.Commands;

public sealed record MarkDailyPlanNotificationsReadCommand(Guid DailyPlanId) : IRequest<Result>;
