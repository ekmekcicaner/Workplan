using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace Workplan.Client.Auth;

public class JwtAuthenticationStateProvider(LocalStorageService localStorage) : AuthenticationStateProvider
{
    public const string AccessTokenKey = "accessToken";
    public const string RefreshTokenKey = "refreshToken";

    private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await localStorage.GetAsync(AccessTokenKey);
        if (string.IsNullOrWhiteSpace(token))
        {
            return Anonymous;
        }

        var identity = TryParseClaimsIdentity(token);
        return identity is null ? Anonymous : new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public void NotifyUserAuthentication(string accessToken)
    {
        var identity = TryParseClaimsIdentity(accessToken) ?? new ClaimsIdentity();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
    }

    public void NotifyUserLogout()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }

    private static readonly HashSet<string> RoleClaimTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ClaimTypes.Role,
        "role",
        "roles"
    };

    private static readonly HashSet<string> NameIdentifierClaimTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ClaimTypes.NameIdentifier,
        "nameid",
        "sub"
    };

    private static ClaimsIdentity? TryParseClaimsIdentity(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length != 3)
        {
            return null;
        }

        var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
        var payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payloadJson);
        if (payload is null)
        {
            return null;
        }

        var claims = new List<Claim>();
        foreach (var (key, value) in payload)
        {
            foreach (var claimValue in ReadClaimValues(value))
            {
                claims.Add(new Claim(key, claimValue));

                if (RoleClaimTypes.Contains(key) && key != ClaimTypes.Role)
                {
                    claims.Add(new Claim(ClaimTypes.Role, claimValue));
                }

                if (NameIdentifierClaimTypes.Contains(key) && key != ClaimTypes.NameIdentifier)
                {
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, claimValue));
                }
            }
        }

        return new ClaimsIdentity(claims, "jwt", ClaimTypes.NameIdentifier, ClaimTypes.Role);
    }

    private static IEnumerable<string> ReadClaimValues(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in value.EnumerateArray())
            {
                yield return item.ToString();
            }
        }
        else
        {
            yield return value.ToString();
        }
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var padded = input.Replace('-', '+').Replace('_', '/');
        padded += (padded.Length % 4) switch
        {
            2 => "==",
            3 => "=",
            _ => ""
        };
        return Convert.FromBase64String(padded);
    }
}
