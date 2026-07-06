using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Auth.Commands;

public sealed record RegisterCommand(
    string Email, string Password, string FullName, IReadOnlyList<string> Roles) : IRequest<Result<Guid>>;
