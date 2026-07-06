using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Auth.Commands;

public sealed record RevokeTokenCommand(string RefreshToken) : IRequest<Result>;
