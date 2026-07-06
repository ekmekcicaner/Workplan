using System.Net.Http.Json;
using Workplan.Client.Json;
using Workplan.Client.Models;

namespace Workplan.Client.Services;

public class AuthApiService(HttpClient http)
{
    public async Task<ApiResult<AuthResult>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("/api/auth/login", request, AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync<AuthResult>(ct);
    }

    public async Task<ApiResult<AuthResult>> RefreshAsync(RefreshRequest request, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("/api/auth/refresh", request, AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync<AuthResult>(ct);
    }

    public async Task<ApiResult<bool>> RevokeAsync(RevokeRequest request, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("/api/auth/revoke", request, AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync(ct);
    }

    public async Task<ApiResult<Guid>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("/api/auth/register", request, AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync<Guid>(ct);
    }

    // /api/auth/me is the one endpoint that isn't wrapped in ApiResponse<T> on the server.
    public async Task<MeResponse?> GetMeAsync(CancellationToken ct = default)
    {
        var response = await http.GetAsync("/api/auth/me", ct);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<MeResponse>(AppJsonOptions.Default, ct)
            : null;
    }
}
