using Mediator;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Auth.Commands;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResultDto>>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(IIdentityService identityService, ITokenService tokenService)
    {
        _identityService = identityService;
        _tokenService = tokenService;
    }

    public async ValueTask<Result<AuthResultDto>> Handle(
        RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var rotated = await _identityService.RotateRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (rotated.IsFailure) return Result<AuthResultDto>.Fail(rotated.Error);

        var (user, refreshToken) = rotated.Value;
        var (accessToken, accessTokenExpiresAtUtc) = _tokenService.CreateAccessToken(user);

        return new AuthResultDto(
            user.Id, user.Email, user.FullName, user.Roles,
            accessToken, accessTokenExpiresAtUtc,
            refreshToken.RawToken, refreshToken.ExpiresAtUtc);
    }
}
