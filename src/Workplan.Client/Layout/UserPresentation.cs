using System.Security.Claims;
using Workplan.Client.Auth;

namespace Workplan.Client.Layout;

internal static class UserPresentation
{
    public static string DisplayName(ClaimsPrincipal user) =>
        user.FindFirst("full_name")?.Value
        ?? user.FindFirst(ClaimTypes.Email)?.Value
        ?? "Kullanıcı";

    public static string Title(ClaimsPrincipal user)
    {
        var titles = user.FindAll(ClaimTypes.Role)
            .Select(claim => Roles.DisplayName(claim.Value))
            .Distinct()
            .ToList();

        return titles.Count > 0 ? string.Join(", ", titles) : "Kullanıcı";
    }

    public static string Initials(ClaimsPrincipal user)
    {
        var displayName = DisplayName(user);
        if (string.IsNullOrWhiteSpace(displayName)) return "?";

        var words = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length >= 2)
        {
            return $"{words[0][0]}{words[^1][0]}".ToUpperInvariant();
        }

        var name = displayName.Split('@')[0];
        return name.Length >= 2 ? name[..2].ToUpperInvariant() : name.ToUpperInvariant();
    }

}
