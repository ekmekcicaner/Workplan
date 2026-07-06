using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Users.Commands;

public sealed record UpdateUserRolesCommand(Guid UserId, IReadOnlyList<string> Roles) : IRequest<Result>;
