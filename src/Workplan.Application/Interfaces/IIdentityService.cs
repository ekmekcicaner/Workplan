using Workplan.SharedKernel.Common;

namespace Workplan.Application.Interfaces;

public sealed record IssuedRefreshToken(string RawToken, DateTime ExpiresAtUtc);

public sealed record UserSummary(Guid Id, string Email, string FullName, IReadOnlyList<string> Roles, bool IsActive);

public interface IIdentityService
{
    Task<Result<Guid>> RegisterAsync(
        string email, string password, string fullName, IReadOnlyList<string> roles, CancellationToken ct);

    Task<IReadOnlyList<UserSummary>> GetUsersAsync(string? role, CancellationToken ct);

    Task<Result<AuthenticatedUser>> ValidateCredentialsAsync(string email, string password, CancellationToken ct);

    Task<IssuedRefreshToken> IssueRefreshTokenAsync(Guid userId, CancellationToken ct);

    Task<Result<(AuthenticatedUser User, IssuedRefreshToken RefreshToken)>> RotateRefreshTokenAsync(
        string rawToken, CancellationToken ct);

    Task<Result> RevokeRefreshTokenAsync(string rawToken, CancellationToken ct);

    Task<Result> UpdateUserRolesAsync(Guid userId, IReadOnlyList<string> roles, CancellationToken ct);

    Task<Result> SetUserActiveAsync(Guid userId, bool isActive, CancellationToken ct);

    Task<Result> ResetPasswordAsync(Guid userId, string newPassword, CancellationToken ct);
}
