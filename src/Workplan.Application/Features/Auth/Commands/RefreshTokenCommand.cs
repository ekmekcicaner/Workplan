using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Auth.Commands;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthResultDto>>;
