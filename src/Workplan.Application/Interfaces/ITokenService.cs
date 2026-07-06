namespace Workplan.Application.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateAccessToken(AuthenticatedUser user);
}
