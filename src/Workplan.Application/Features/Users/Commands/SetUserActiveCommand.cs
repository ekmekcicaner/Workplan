using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Users.Commands;

public sealed record SetUserActiveCommand(Guid UserId, bool IsActive) : IRequest<Result>;
