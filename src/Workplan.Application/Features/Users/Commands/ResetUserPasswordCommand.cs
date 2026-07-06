using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Users.Commands;

public sealed record ResetUserPasswordCommand(Guid UserId, string NewPassword) : IRequest<Result>;
