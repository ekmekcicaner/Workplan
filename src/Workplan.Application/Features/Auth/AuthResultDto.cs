namespace Workplan.Application.Features.Auth;

public sealed record AuthResultDto(
    Guid UserId,
    string Email,
    string FullName,
    IReadOnlyList<string> Roles,
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);
