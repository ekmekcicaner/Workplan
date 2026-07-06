using System.Net.Http.Json;
using Workplan.Client.Json;
using Workplan.Client.Models;

namespace Workplan.Client.Services;

public class LocationsApiService(HttpClient http)
{
    public async Task<ApiResult<Guid>> CreateAsync(CreateLocationRequest request, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("/api/locations/", request, AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync<Guid>(ct);
    }

    public async Task<ApiResult<bool>> AssignHeadOfMasterAsync(Guid id, AssignHeadOfMasterRequest request, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync($"/api/locations/{id}/assign-head-of-master", request, AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync(ct);
    }

    public async Task<ApiResult<List<LocationDto>>> GetByRegionAsync(
        Guid crewRegionId, bool includeInactive = false, CancellationToken ct = default)
    {
        var response = await http.GetAsync($"/api/locations/by-region/{crewRegionId}?includeInactive={includeInactive}", ct);
        return await response.ToApiResultAsync<List<LocationDto>>(ct);
    }

    public async Task<ApiResult<LocationDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var response = await http.GetAsync($"/api/locations/{id}", ct);
        return await response.ToApiResultAsync<LocationDto>(ct);
    }

    public async Task<ApiResult<bool>> UpdateAsync(Guid id, UpdateLocationRequest request, CancellationToken ct = default)
    {
        var response = await http.PutAsJsonAsync($"/api/locations/{id}", request, AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync(ct);
    }

    public async Task<ApiResult<bool>> SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync(
            $"/api/locations/{id}/activation", new SetActiveRequest(isActive), AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync(ct);
    }
}
