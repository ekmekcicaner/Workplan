using Mediator;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Auth.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResultDto>>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(IIdentityService identityService, ITokenService tokenService)
    {
        _identityService = identityService;
        _tokenService = tokenService;
    }

    public async ValueTask<Result<AuthResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var credentials = await _identityService.ValidateCredentialsAsync(request.Email, request.Password, cancellationToken);
        if (credentials.IsFailure) return Result<AuthResultDto>.Fail(credentials.Error);

        var user = credentials.Value;
        var (accessToken, accessTokenExpiresAtUtc) = _tokenService.CreateAccessToken(user);
        var refreshToken = await _identityService.IssueRefreshTokenAsync(user.Id, cancellationToken);

        return new AuthResultDto(
            user.Id, user.Email, user.FullName, user.Roles,
            accessToken, accessTokenExpiresAtUtc,
            refreshToken.RawToken, refreshToken.ExpiresAtUtc);
    }
}
