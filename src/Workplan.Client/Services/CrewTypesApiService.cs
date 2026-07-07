using System.Net.Http.Json;
using Workplan.Client.Json;
using Workplan.Client.Models;

namespace Workplan.Client.Services;

public class CrewTypesApiService(HttpClient http)
{
    public async Task<ApiResult<List<CrewTypeDto>>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var response = await http.GetAsync($"/api/crew-types/?includeInactive={includeInactive}", ct);
        return await response.ToApiResultAsync<List<CrewTypeDto>>(ct);
    }

    public async Task<ApiResult<Guid>> CreateAsync(CreateCrewTypeRequest request, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("/api/crew-types/", request, AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync<Guid>(ct);
    }

    public async Task<ApiResult<bool>> UpdateAsync(Guid id, UpdateCrewTypeRequest request, CancellationToken ct = default)
    {
        var response = await http.PutAsJsonAsync($"/api/crew-types/{id}", request, AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync(ct);
    }

    public async Task<ApiResult<bool>> SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync(
            $"/api/crew-types/{id}/activation", new SetActiveRequest(isActive), AppJsonOptions.Default, ct);
        return await response.ToApiResultAsync(ct);
    }
}
