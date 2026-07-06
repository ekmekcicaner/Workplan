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

    // The JWT embeds the long-form ClaimTypes.NameIdentifier/ClaimTypes.Role URIs (that's what
    // JwtTokenService puts in on the server); MapInboundClaims=false there only affects the
    // server's own inbound processing, not what's literally inside the issued token.
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
            if (value.ValueKind == JsonValueKind.Array)
            {
                claims.AddRange(value.EnumerateArray().Select(item => new Claim(key, item.ToString())));
            }
            else
            {
                claims.Add(new Claim(key, value.ToString()));
            }
        }

        return new ClaimsIdentity(claims, "jwt", ClaimTypes.NameIdentifier, ClaimTypes.Role);
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
