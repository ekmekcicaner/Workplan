using Mediator;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Auth.Commands;

public class RevokeTokenCommandHandler(IIdentityService identityService) : IRequestHandler<RevokeTokenCommand, Result>
{
    public async ValueTask<Result> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
        => await identityService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);
}
