using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Workplan.Application.Interfaces;
using Workplan.Infrastructure.Persistence;
using Workplan.SharedKernel.Common;

namespace Workplan.Infrastructure.Identity;

public class IdentityService(UserManager<ApplicationUser> userManager, AppDbContext db, IOptions<JwtOptions> jwtOptions)
    : IIdentityService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<Result<Guid>> RegisterAsync(
        string email, string password, string fullName, IReadOnlyList<string> roles, CancellationToken ct)
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = email, Email = email, FullName = fullName };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
            return Result<Guid>.Fail(Error.Validation(
                "Kullanıcı oluşturulamadı.", createResult.Errors.Select(e => e.Description)));

        var roleResult = await userManager.AddToRolesAsync(user, roles);
        if (!roleResult.Succeeded)
            return Result<Guid>.Fail(Error.Validation(
                "Rol ataması başarısız.", roleResult.Errors.Select(e => e.Description)));

        return user.Id;
    }

    public async Task<IReadOnlyList<UserSummary>> GetUsersAsync(string? role, CancellationToken ct)
    {
        var users = string.IsNullOrWhiteSpace(role)
            ? await userManager.Users.AsNoTracking().ToListAsync(ct)
            : (await userManager.GetUsersInRoleAsync(role)).ToList();

        var summaries = new List<UserSummary>(users.Count);
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            var isActive = !await userManager.IsLockedOutAsync(user);
            summaries.Add(new UserSummary(user.Id, user.Email!, user.FullName, roles.ToList(), isActive));
        }

        return summaries;
    }

    public async Task<IReadOnlyDictionary<Guid, string>> GetDisplayNamesAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken ct)
    {
        if (userIds.Count == 0)
            return new Dictionary<Guid, string>();

        var distinctIds = userIds.Where(id => id != Guid.Empty).Distinct().ToList();
        if (distinctIds.Count == 0)
            return new Dictionary<Guid, string>();

        return await userManager.Users
            .AsNoTracking()
            .Where(user => distinctIds.Contains(user.Id))
            .Select(user => new { user.Id, user.FullName })
            .ToDictionaryAsync(user => user.Id, user => user.FullName, ct);
    }

    public async Task<Result<AuthenticatedUser>> ValidateCredentialsAsync(
        string email, string password, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || !await userManager.CheckPasswordAsync(user, password))
            return Result<AuthenticatedUser>.Fail(Error.Unauthorized("Email veya şifre hatalı."));

        if (await userManager.IsLockedOutAsync(user))
            return Result<AuthenticatedUser>.Fail(Error.Unauthorized("Kullanıcı devre dışı bırakılmış."));

        var roles = await userManager.GetRolesAsync(user);
        return new AuthenticatedUser(user.Id, user.Email!, user.FullName, roles.ToList());
    }

    public async Task<Result> UpdateUserRolesAsync(Guid userId, IReadOnlyList<string> roles, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Result.Fail(Error.NotFound("Kullanıcı bulunamadı."));

        var currentRoles = await userManager.GetRolesAsync(user);

        var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
            return Result.Fail(Error.Validation(
                "Mevcut roller kaldırılamadı.", removeResult.Errors.Select(e => e.Description)));

        var addResult = await userManager.AddToRolesAsync(user, roles);
        if (!addResult.Succeeded)
            return Result.Fail(Error.Validation(
                "Rol ataması başarısız.", addResult.Errors.Select(e => e.Description)));

        return Result.Ok();
    }

    public async Task<Result> SetUserActiveAsync(Guid userId, bool isActive, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Result.Fail(Error.NotFound("Kullanıcı bulunamadı."));

        await userManager.SetLockoutEnabledAsync(user, true);
        var lockoutResult = await userManager.SetLockoutEndDateAsync(
            user, isActive ? null : DateTimeOffset.MaxValue);

        if (!lockoutResult.Succeeded)
            return Result.Fail(Error.Validation(
                "Kullanıcı durumu güncellenemedi.", lockoutResult.Errors.Select(e => e.Description)));

        return Result.Ok();
    }

    public async Task<Result> ResetPasswordAsync(Guid userId, string newPassword, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Result.Fail(Error.NotFound("Kullanıcı bulunamadı."));

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, newPassword);

        if (!result.Succeeded)
            return Result.Fail(Error.Validation(
                "Şifre sıfırlanamadı.", result.Errors.Select(e => e.Description)));

        return Result.Ok();
    }

    public async Task<IssuedRefreshToken> IssueRefreshTokenAsync(Guid userId, CancellationToken ct)
    {
        var rawToken = GenerateRawToken();
        var expiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);

        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = Hash(rawToken),
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = expiresAtUtc
        });
        await db.SaveChangesAsync(ct);

        return new IssuedRefreshToken(rawToken, expiresAtUtc);
    }

    public async Task<Result<(AuthenticatedUser User, IssuedRefreshToken RefreshToken)>> RotateRefreshTokenAsync(
        string rawToken, CancellationToken ct)
    {
        var hash = Hash(rawToken);
        var existing = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
        if (existing is null || !existing.IsActive)
            return Result<(AuthenticatedUser, IssuedRefreshToken)>.Fail(
                Error.Unauthorized("Refresh token geçersiz veya süresi dolmuş."));

        var user = await userManager.FindByIdAsync(existing.UserId.ToString());
        if (user is null)
            return Result<(AuthenticatedUser, IssuedRefreshToken)>.Fail(Error.Unauthorized("Kullanıcı bulunamadı."));

        var newIssued = await IssueRefreshTokenAsync(user.Id, ct);

        existing.RevokedAtUtc = DateTime.UtcNow;
        existing.ReplacedByTokenHash = Hash(newIssued.RawToken);
        await db.SaveChangesAsync(ct);

        var roles = await userManager.GetRolesAsync(user);
        var authenticatedUser = new AuthenticatedUser(user.Id, user.Email!, user.FullName, roles.ToList());

        return (authenticatedUser, newIssued);
    }

    public async Task<Result> RevokeRefreshTokenAsync(string rawToken, CancellationToken ct)
    {
        var hash = Hash(rawToken);
        var existing = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
        if (existing is null || !existing.IsActive)
            return Result.Fail(Error.Unauthorized("Refresh token geçersiz veya süresi dolmuş."));

        existing.RevokedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }

    private static string GenerateRawToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    private static string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }
}
