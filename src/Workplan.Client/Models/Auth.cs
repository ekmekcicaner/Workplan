namespace Workplan.Client.Models;

public record LoginRequest(string Email, string Password);

public record RefreshRequest(string RefreshToken);

public record RevokeRequest(string RefreshToken);

public record RegisterRequest(string Email, string Password, string FullName, IReadOnlyList<string> Roles);

public record AuthResult(
    Guid UserId,
    string Email,
    string FullName,
    IReadOnlyList<string> Roles,
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);

public record MeResponse(Guid UserId, IReadOnlyList<string> Roles);
