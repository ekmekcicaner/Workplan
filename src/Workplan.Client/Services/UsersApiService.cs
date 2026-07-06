using System.Net.Http.Json;
using Workplan.Client.Json;
using Workplan.Client.Models;

namespace Workplan.Client.Services;

public class UsersApiService(HttpClient http)
{
    public async Task<ApiResult<List<UserDto>>> GetUsersAsync(string? role = null, CancellationToken ct = default)
    {
        var url = string.IsNullOrWhiteSpace(role) ? "/api/users" : $"/api/users?role={Uri.EscapeDataString(role)}";
        var response = await http.GetAsync(url, ct);
        return await response.ToApiResultAsync<List<UserDto>>(ct);
    }

    public async Task<ApiResult<Guid>> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("/api/users", request, AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync<Guid>(ct);
    }

    public async Task<ApiResult<bool>> UpdateRolesAsync(Guid id, UpdateUserRolesRequest request, CancellationToken ct = default)
    {
        var response = await http.PutAsJsonAsync($"/api/users/{id}/roles", request, AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync(ct);
    }

    public async Task<ApiResult<bool>> SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync(
            $"/api/users/{id}/activation", new SetActiveRequest(isActive), AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync(ct);
    }

    public async Task<ApiResult<bool>> ResetPasswordAsync(Guid id, ResetPasswordRequest request, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync($"/api/users/{id}/reset-password", request, AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync(ct);
    }
}
