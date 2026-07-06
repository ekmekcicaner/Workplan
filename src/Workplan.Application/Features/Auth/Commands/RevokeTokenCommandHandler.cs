using Mediator;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Auth.Commands;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Result>
{
    private readonly IIdentityService _identityService;

    public RevokeTokenCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async ValueTask<Result> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
        => await _identityService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);
}
