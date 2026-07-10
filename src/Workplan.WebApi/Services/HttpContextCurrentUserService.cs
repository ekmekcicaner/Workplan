using System.Security.Claims;
using Workplan.Application.Interfaces;

namespace Workplan.WebApi.Services;

public sealed class HttpContextCurrentUserService(
    IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?
                .User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var userId)
                ? userId
                : null;
        }
    }

    public IReadOnlyList<string> Roles =>
        httpContextAccessor.HttpContext?
            .User
            .FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .ToArray()
        ?? [];
}