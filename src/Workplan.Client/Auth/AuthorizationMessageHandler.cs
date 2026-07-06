using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Workplan.Client.Json;
using Workplan.Client.Models;

namespace Workplan.Client.Auth;

public class AuthorizationMessageHandler(
    LocalStorageService localStorage,
    JwtAuthenticationStateProvider authStateProvider,
    IConfiguration configuration) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await AttachTokenAsync(request);

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return response;
        }

        var refreshToken = await localStorage.GetAsync(JwtAuthenticationStateProvider.RefreshTokenKey);
        if (string.IsNullOrEmpty(refreshToken))
        {
            return response;
        }

        var refreshed = await TryRefreshAsync(refreshToken, cancellationToken);
        if (refreshed is null)
        {
            await ClearTokensAsync();
            return response;
        }

        await localStorage.SetAsync(JwtAuthenticationStateProvider.AccessTokenKey, refreshed.AccessToken);
        await localStorage.SetAsync(JwtAuthenticationStateProvider.RefreshTokenKey, refreshed.RefreshToken);
        authStateProvider.NotifyUserAuthentication(refreshed.AccessToken);

        var retryRequest = await CloneRequestAsync(request);
        retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshed.AccessToken);
        response.Dispose();
        return await base.SendAsync(retryRequest, cancellationToken);
    }

    private async Task AttachTokenAsync(HttpRequestMessage request)
    {
        var accessToken = await localStorage.GetAsync(JwtAuthenticationStateProvider.AccessTokenKey);
        if (!string.IsNullOrEmpty(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }

    private async Task<AuthResult?> TryRefreshAsync(string refreshToken, CancellationToken ct)
    {
        // Uses a bare HttpClient (not the DI "Api" client) so the refresh call doesn't
        // recurse back through this same handler.
        using var rawClient = new HttpClient { BaseAddress = new Uri(configuration["ApiBaseUrl"]!) };
        HttpResponseMessage response;
        try
        {
            response = await rawClient.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest(refreshToken), AppJsonOptions.Default, ct);
        }
        catch (HttpRequestException)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResult>>(AppJsonOptions.Default, ct);
        return body is { Success: true } ? body.Data : null;
    }

    private async Task ClearTokensAsync()
    {
        await localStorage.RemoveAsync(JwtAuthenticationStateProvider.AccessTokenKey);
        await localStorage.RemoveAsync(JwtAuthenticationStateProvider.RefreshTokenKey);
        authStateProvider.NotifyUserLogout();
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        if (request.Content is not null)
        {
            var buffer = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(buffer);
            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}
