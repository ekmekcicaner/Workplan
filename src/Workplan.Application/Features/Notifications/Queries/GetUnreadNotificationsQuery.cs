using Mediator;
using Workplan.Application.Features.Notifications;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Notifications.Queries;

public sealed record GetUnreadNotificationsQuery : IRequest<Result<List<NotificationDto>>>;
