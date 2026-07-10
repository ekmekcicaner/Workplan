using Mediator;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Auth.Commands;

public class LoginCommandHandler(IIdentityService identityService, ITokenService tokenService)
    : IRequestHandler<LoginCommand, Result<AuthResultDto>>
{
    public async ValueTask<Result<AuthResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var credentials = await identityService.ValidateCredentialsAsync(request.Email, request.Password, cancellationToken);
        if (credentials.IsFailure) return Result<AuthResultDto>.Fail(credentials.Error);

        var user = credentials.Value;
        var (accessToken, accessTokenExpiresAtUtc) = tokenService.CreateAccessToken(user);
        var refreshToken = await identityService.IssueRefreshTokenAsync(user.Id, cancellationToken);

        return new AuthResultDto(
            user.Id, user.Email, user.FullName, user.Roles,
            accessToken, accessTokenExpiresAtUtc,
            refreshToken.RawToken, refreshToken.ExpiresAtUtc);
    }
}
