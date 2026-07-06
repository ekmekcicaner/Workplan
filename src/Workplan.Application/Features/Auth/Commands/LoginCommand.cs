using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Auth.Commands;

public sealed record LoginCommand(string Email, string Password) : IRequest<Result<AuthResultDto>>;
